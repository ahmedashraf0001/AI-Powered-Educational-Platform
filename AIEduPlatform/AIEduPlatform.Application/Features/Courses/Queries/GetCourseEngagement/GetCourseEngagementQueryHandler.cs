using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.GetCourseEngagement
{
    public class GetCourseEngagementQueryHandler
        : IRequestHandler<GetCourseEngagementQuery, CourseEngagementReport>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetCourseEngagementQueryHandler> _logger;

        public GetCourseEngagementQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            ILogger<GetCourseEngagementQueryHandler> logger)
        {
            _uow = uow;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<CourseEngagementReport> Handle(
            GetCourseEngagementQuery request,
            CancellationToken ct)
        {
            var teacherId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            // ── Verify course ownership ─────────────────────────────────────
            var course = await _uow.Courses.GetByIdAsync(request.CourseId, ct)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            if (course.TeacherId != teacherId)
                throw new ForbiddenException("You are not the teacher of this course.");

            // ── Get active enrollments with student info ─────────────────────
            var enrollments = await _uow.Enrollments
                .GetEnrollmentsByCourseAsync(request.CourseId, includeStudent: true, ct: ct);

            if (enrollments.Count == 0)
            {
                return new CourseEngagementReport
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    TotalEnrolled = 0,
                    Students = []
                };
            }

            // ── Count available exams for this course ────────────────────────
            var totalExams = await _uow.Exams.CountAsync(
                e => e.CourseId == request.CourseId, ct);

            // ── Build per-student stats ──────────────────────────────────────
            var students = new List<StudentEngagementDto>();
            var now = DateTime.UtcNow;

            foreach (var enrollment in enrollments)
            {
                var sid = enrollment.StudentId;
                var student = enrollment.Student;

                // Study session stats for this student+course
                var sessionStats = await _uow.StudySessions
                    .GetStudentStatsAsync(sid, request.CourseId, ct);

                // Grade stats for this student+course
                var gradeStats = await _uow.Grades
                    .GetStudentStatsAsync(sid, request.CourseId, ct);

                // Count submissions for this student in this course's exams
                var submissions = await _uow.Submissions
                    .GetSubmissionsByStudentAndCourseAsync(sid, request.CourseId, includeGrade: true, ct);

                var pendingSubmissions = submissions.Count(s => s.Grade == null);

                // Days since last activity
                var lastActivityDate = sessionStats.LastSessionDate;
                var daysSinceLastActivity = lastActivityDate.HasValue
                    ? (int)(now - lastActivityDate.Value).TotalDays
                    : (int)(now - enrollment.EnrolledAt).TotalDays;

                // ── Compute engagement score (0-100) ────────────────────────
                var engagementScore = ComputeEngagementScore(
                    totalStudySessions: sessionStats.TotalSessions,
                    totalStudyHours: sessionStats.TotalStudyTime.TotalHours,
                    totalMessages: sessionStats.TotalMessages,
                    flashcards: sessionStats.TotalFlashcards,
                    quizzes: sessionStats.TotalQuizzes,
                    mindMaps: sessionStats.TotalMindMaps,
                    examsTaken: gradeStats.TotalExamsTaken,
                    examsAvailable: totalExams,
                    avgScore: gradeStats.AverageScore,
                    daysSinceLastActivity: daysSinceLastActivity);

                students.Add(new StudentEngagementDto
                {
                    StudentId = sid,
                    StudentName = $"{student?.FirstName} {student?.LastName}".Trim(),
                    Email = student?.Email ?? string.Empty,
                    EnrolledAt = enrollment.EnrolledAt,
                    EnrollmentStatus = enrollment.Status.ToString(),

                    TotalStudySessions = sessionStats.TotalSessions,
                    TotalStudyHours = Math.Round(sessionStats.TotalStudyTime.TotalHours, 1),
                    LastStudySessionDate = sessionStats.LastSessionDate,
                    DaysSinceLastActivity = daysSinceLastActivity,

                    TotalChatMessages = sessionStats.TotalMessages,
                    TotalFlashcardsGenerated = sessionStats.TotalFlashcards,
                    TotalQuizzesTaken = sessionStats.TotalQuizzes,
                    TotalMindMapsGenerated = sessionStats.TotalMindMaps,

                    ExamsTaken = gradeStats.TotalExamsTaken,
                    ExamsAvailable = totalExams,
                    AverageExamScore = gradeStats.AverageScore,
                    PendingSubmissions = pendingSubmissions,

                    EngagementScore = engagementScore,
                    EngagementLevel = engagementScore switch
                    {
                        <= 25 => EngagementLevel.Critical,
                        <= 50 => EngagementLevel.Low,
                        <= 75 => EngagementLevel.Moderate,
                        _ => EngagementLevel.High
                    }
                });
            }

            // Sort so worst-engaged students are first (teacher sees at-risk first)
            students = students.OrderBy(s => s.EngagementScore).ToList();

            var atRisk = students.Count(s =>
                s.EngagementLevel is EngagementLevel.Critical or EngagementLevel.Low);

            var report = new CourseEngagementReport
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                TotalEnrolled = students.Count,
                ActiveStudents = students.Count(s => s.EnrollmentStatus == "Active"),
                AtRiskStudents = atRisk,
                AverageEngagementScore = students.Count > 0
                    ? Math.Round(students.Average(s => s.EngagementScore), 1) : 0,
                Students = students
            };

            _logger.LogInformation(
                "Course engagement report generated. CourseId: {CourseId}, Enrolled: {Total}, AtRisk: {AtRisk}",
                request.CourseId, report.TotalEnrolled, atRisk);

            return report;
        }

        /// <summary>
        /// Weighted engagement score (0-100) based on multiple activity dimensions.
        /// </summary>
        private static int ComputeEngagementScore(
            int totalStudySessions,
            double totalStudyHours,
            int totalMessages,
            int flashcards,
            int quizzes,
            int mindMaps,
            int examsTaken,
            int examsAvailable,
            float avgScore,
            int daysSinceLastActivity)
        {
            // 1. Study frequency (0-25 pts)  — at least 5 sessions = full marks
            var sessionPts = Math.Min(totalStudySessions / 5.0, 1.0) * 25;

            // 2. Study depth (0-20 pts) — messages + flashcards + quizzes + mind maps
            var interactionCount = totalMessages + flashcards + quizzes + mindMaps;
            var depthPts = Math.Min(interactionCount / 20.0, 1.0) * 20;

            // 3. Exam participation (0-20 pts) — ratio of exams taken vs available
            var examPts = examsAvailable > 0
                ? (examsTaken / (double)examsAvailable) * 20
                : 20; // no exams yet → full marks (can't penalise)

            // 4. Exam performance (0-15 pts)
            var perfPts = examsTaken > 0
                ? Math.Min(avgScore / 100.0, 1.0) * 15
                : 0;

            // 5. Recency (0-20 pts) — penalise inactivity
            var recencyPts = daysSinceLastActivity switch
            {
                <= 2 => 20,
                <= 7 => 15,
                <= 14 => 10,
                <= 30 => 5,
                _ => 0
            };

            var raw = sessionPts + depthPts + examPts + perfPts + recencyPts;
            return (int)Math.Round(Math.Clamp(raw, 0, 100));
        }
    }
}
