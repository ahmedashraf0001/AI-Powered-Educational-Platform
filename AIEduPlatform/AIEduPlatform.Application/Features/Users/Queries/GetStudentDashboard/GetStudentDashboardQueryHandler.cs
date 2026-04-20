using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Users.Queries.GetStudentDashboard
{
    public class GetStudentDashboardQueryHandler : IRequestHandler<GetStudentDashboardQuery, StudentDashboardDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentDashboardQueryHandler> _logger;

        public GetStudentDashboardQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentDashboardQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<StudentDashboardDto> Handle(GetStudentDashboardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var studentId = userId.Value;

            // 1. Course progress
            var allEnrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(studentId, ct: cancellationToken);
            var enrollments = allEnrollments
                .Where(e => e.Status != Core.Domain.Enums.EnrollmentStatus.Dropped)
                .ToList();
            var courseProgressList = new List<CourseProgressSummary>();

            foreach (var enrollment in enrollments)
            {
                var course = await _unitOfWork.Courses.GetCourseByIdAsync(enrollment.CourseId, new CourseIncludeOptions { IncludeLectures = true, IncludeMaterials = true }, cancellationToken);
                if (course == null) continue;

                var totalMaterials = course.Lectures?.SelectMany(l => l.Materials ?? Enumerable.Empty<Core.Domain.Entities.Material>()).Count() ?? 0;
                var completedMaterials = await _unitOfWork.MaterialProgress.GetCompletedMaterialCountAsync(studentId, enrollment.CourseId, cancellationToken);

                courseProgressList.Add(new CourseProgressSummary
                {
                    CourseId = enrollment.CourseId,
                    CourseTitle = course.Title,
                    Status = enrollment.Status.ToString(),
                    CompletedMaterials = completedMaterials,
                    TotalMaterials = totalMaterials,
                    ProgressPercentage = totalMaterials > 0 ? Math.Round((double)completedMaterials / totalMaterials * 100, 1) : 0,
                    EnrolledAt = enrollment.EnrolledAt
                });
            }

            // 2. Engagement analytics
            var studySessions = await _unitOfWork.StudySessions.GetSessionsByStudentIdAsync(studentId, cancellationToken);
            var materialsViewed = 0;
            foreach (var enrollment in enrollments)
            {
                var progressList = await _unitOfWork.MaterialProgress.GetProgressByCourseAsync(studentId, enrollment.CourseId, cancellationToken);
                materialsViewed += progressList.Count;
            }

            var totalTimeMinutes = studySessions
                .Where(s => s.EndedAt.HasValue)
                .Sum(s => (s.EndedAt!.Value - s.StartedAt).TotalMinutes);

            var totalQuizzes = studySessions.Sum(s => s.GeneratedQuizzes?.Count ?? 0);
            var totalFlashcards = studySessions.Sum(s => s.Flashcards?.Count ?? 0);

            var engagement = new EngagementAnalytics
            {
                TotalStudySessions = studySessions.Count,
                TotalMaterialsViewed = materialsViewed,
                TotalTimeSpentMinutes = Math.Round(totalTimeMinutes, 1),
                TotalQuizzesGenerated = totalQuizzes,
                TotalFlashcardsGenerated = totalFlashcards,
                CoursesEnrolled = enrollments.Count,
                CoursesCompleted = enrollments.Count(e => e.Status == Core.Domain.Enums.EnrollmentStatus.Completed)
            };

            // 3. Academic performance
            var gradeStats = await _unitOfWork.Grades.GetStudentStatsAsync(studentId, ct: cancellationToken);
            var performance = new AcademicPerformance
            {
                ExamsTaken = gradeStats.TotalExamsTaken,
                AverageScore = gradeStats.AverageScore,
                HighestScore = gradeStats.HighestScore,
                LowestScore = gradeStats.LowestScore
            };

            // 4. Grade trend (scores grouped by month)
            var allGrades = await _unitOfWork.Grades.GetGradesByStudentIdAsync(studentId, includeSubmission: true, ct: cancellationToken);
            var gradeTrend = allGrades
                .Where(g => g.Submission != null)
                .GroupBy(g => g.Submission.SubmittedAt.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .Select(g => new GradeTrendPoint
                {
                    Month = g.Key,
                    AverageScore = g.Average(x => x.Score),
                    ExamCount = g.Count()
                })
                .ToList();

            // 5. Submission history
            var submissions = await _unitOfWork.Submissions.GetSubmissionsByStudentIdAsync(
                studentId, includeExam: true, includeGrade: true, ct: cancellationToken);

            var submissionHistory = submissions
                .OrderByDescending(s => s.SubmittedAt)
                .Take(20)
                .Select(s => new SubmissionHistoryItem
                {
                    SubmissionId = s.Id,
                    ExamTitle = s.Exam?.Title ?? "Unknown Exam",
                    CourseName = s.Exam?.Course?.Title ?? "Unknown Course",
                    Score = s.Grade?.Score,
                    SubmittedAt = s.SubmittedAt,
                    IsGraded = s.Grade != null
                })
                .ToList();

            _logger.LogInformation("Student dashboard loaded for user {UserId}: {Courses} courses, {Exams} exams",
                studentId, courseProgressList.Count, performance.ExamsTaken);

            // 6. Summary counts
            var totalLectures = courseProgressList.Sum(c => c.TotalMaterials);
            var totalLecturesCompleted = courseProgressList.Sum(c => c.CompletedMaterials);
            var completedCourses = courseProgressList.Count(c => c.TotalMaterials > 0 && c.CompletedMaterials >= c.TotalMaterials);
            var overallProgress = totalLectures > 0
                ? Math.Round((double)totalLecturesCompleted / totalLectures * 100, 1)
                : 0;

            // 7. Recent activity — last completed materials
            var recentActivity = new List<RecentActivityItem>();
            foreach (var enrollment in enrollments.Take(5))
            {
                var progressList = await _unitOfWork.MaterialProgress.GetProgressByCourseAsync(
                    studentId, enrollment.CourseId, cancellationToken);
                var course = courseProgressList.FirstOrDefault(c => c.CourseId == enrollment.CourseId);
                foreach (var mp in progressList.Where(p => p.IsCompleted).OrderByDescending(p => p.UpdatedAt).Take(3))
                {
                    recentActivity.Add(new RecentActivityItem
                    {
                        CourseTitle = course?.CourseTitle ?? string.Empty,
                        LectureTitle = mp.Material?.Title ?? string.Empty,
                        CompletedAt = mp.UpdatedAt
                    });
                }
            }
            recentActivity = recentActivity.OrderByDescending(a => a.CompletedAt).Take(10).ToList();

            // Calculate Study Streak Data
            var streakData = new StudyStreakData();
            var today = DateTime.UtcNow.Date;
            var activeDates = studySessions.Select(s => s.StartedAt.Date).Distinct().ToList();

            int currentStreak = 0;
            var checkDate = today;
            if (!activeDates.Contains(today)) {
                checkDate = today.AddDays(-1);
            }
            while (activeDates.Contains(checkDate)) {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            streakData.CurrentStreak = currentStreak;

            var currentDayOfWeek = (int)today.DayOfWeek;
            int diff = (7 + (currentDayOfWeek - 1)) % 7; 
            var startOfWeek = today.AddDays(-1 * diff);

            for (int i = 0; i < 7; i++) {
                streakData.ActiveDays.Add(activeDates.Contains(startOfWeek.AddDays(i)));
            }

            return new StudentDashboardDto
            {
                Streak = streakData,
                TotalEnrolledCourses = enrollments.Count,
                CompletedCourses = completedCourses,
                InProgressCourses = enrollments.Count - completedCourses,
                TotalLecturesCompleted = totalLecturesCompleted,
                TotalLectures = totalLectures,
                OverallProgressPercentage = overallProgress,
                CertificatesEarned = completedCourses, // 1 certificate per completed course
                CourseProgress = courseProgressList,
                Engagement = engagement,
                Performance = performance,
                GradeTrend = gradeTrend,
                SubmissionHistory = submissionHistory,
                RecentActivity = recentActivity
            };
        }
    }
}

