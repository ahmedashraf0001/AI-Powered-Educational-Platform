using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Identity;
using UserEntity = AIEduPlatform.Core.Domain.Entities.User;

namespace AIEduPlatform.Application.Features.Users.Queries.GetTeacherDashboard
{
    public class GetTeacherDashboardQueryHandler : IRequestHandler<GetTeacherDashboardQuery, TeacherDashboardStats>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly UserManager<UserEntity> _userManager;

        public GetTeacherDashboardQueryHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            UserManager<UserEntity> userManager)
        {
            _uow = uow;
            _currentUser = currentUser;
            _userManager = userManager;
        }

        public async Task<TeacherDashboardStats> Handle(GetTeacherDashboardQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user);
            if (!roles.Contains("Teacher"))
                throw new ForbiddenException("You must be a teacher to access the dashboard.");

            // Get teacher's courses with includes
            var teacherCourses = (await _uow.Courses.FindAsync(
                c => c.TeacherId == userId, cancellationToken)).ToList();

            var courseIds = teacherCourses.Select(c => c.Id).ToList();

            // Enrollment data
            var allEnrollments = new List<Enrollment>();
            foreach (var courseId in courseIds)
            {
                var courseEnrollments = await _uow.Enrollments.GetEnrollmentsByCourseAsync(
                    courseId, includeStudent: true, cancellationToken);
                allEnrollments.AddRange(courseEnrollments);
            }

            var totalEnrollments = allEnrollments.Count;
            var totalStudents = allEnrollments.Select(e => e.StudentId).Distinct().Count();

            // Revenue from enrollments (AmountPaid tracks what each student paid)
            var totalRevenue = allEnrollments
                .Where(e => e.Status == EnrollmentStatus.Active)
                .Sum(e => e.AmountPaid);

            // Reviews
            var allReviews = new List<Review>();
            foreach (var courseId in courseIds)
            {
                var reviews = await _uow.Reviews.GetByCourseIdAsync(courseId, cancellationToken);
                allReviews.AddRange(reviews);
            }
            var totalReviews = allReviews.Count;
            var averageRating = totalReviews > 0 ? Math.Round(allReviews.Average(r => r.Rating), 2) : 0;

            // Lectures count
            var totalLectures = 0;
            foreach (var courseId in courseIds)
            {
                var lectures = await _uow.Lectures.FindAsync(
                    l => l.CourseId == courseId, cancellationToken);
                totalLectures += lectures.Count();
            }

            // Exams, grades
            var totalExams = await _uow.Exams.CountAsync(
                e => courseIds.Contains(e.CourseId), cancellationToken);

            var pendingApprovals = await _uow.Grades.CountAsync(
                g => g.IsAiGraded && !g.IsApproved
                    && courseIds.Contains(g.Submission.Exam.CourseId), cancellationToken);

            var ungradedSubmissions = await _uow.Submissions.CountAsync(
                s => s.Grade == null
                    && courseIds.Contains(s.Exam.CourseId), cancellationToken);

            // Completion rate — completedMaterials / totalMaterials across all students
            var totalMaterialsAllCourses = 0;
            var completedMaterialsAllStudents = 0;
            foreach (var courseId in courseIds)
            {
                var totalMat = await _uow.Courses.GetMaterialsCountAsync(courseId, cancellationToken);
                var studentIds = allEnrollments
                    .Where(e => e.CourseId == courseId)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();

                foreach (var sid in studentIds)
                {
                    totalMaterialsAllCourses += totalMat;
                    completedMaterialsAllStudents += await _uow.MaterialProgress.GetCompletedMaterialCountAsync(
                        sid, courseId, cancellationToken);
                }
            }
            var completionRate = totalMaterialsAllCourses > 0
                ? Math.Round((double)completedMaterialsAllStudents / totalMaterialsAllCourses * 100, 1)
                : 0;

            // Recent enrollments (last 10)
            var recentEnrollments = allEnrollments
                .OrderByDescending(e => e.EnrolledAt)
                .Take(10)
                .Select(e => new RecentEnrollmentItem
                {
                    StudentName = e.Student?.UserName ?? string.Empty,
                    CourseName = teacherCourses.FirstOrDefault(c => c.Id == e.CourseId)?.Title ?? string.Empty,
                    EnrolledAt = e.EnrolledAt
                })
                .ToList();

            // Course performance
            var coursePerformance = new List<CoursePerformanceItem>();
            foreach (var course in teacherCourses)
            {
                var courseEnrollments = allEnrollments.Count(e => e.CourseId == course.Id);
                var courseReviews = allReviews.Where(r => r.CourseId == course.Id).ToList();
                var courseAvgRating = courseReviews.Count > 0 ? Math.Round(courseReviews.Average(r => r.Rating), 2) : 0;

                var courseTotalMat = await _uow.Courses.GetMaterialsCountAsync(course.Id, cancellationToken);
                var courseStudentIds = allEnrollments
                    .Where(e => e.CourseId == course.Id)
                    .Select(e => e.StudentId)
                    .Distinct()
                    .ToList();
                var courseTotalAll = 0;
                var courseCompletedAll = 0;
                foreach (var sid in courseStudentIds)
                {
                    courseTotalAll += courseTotalMat;
                    courseCompletedAll += await _uow.MaterialProgress.GetCompletedMaterialCountAsync(
                        sid, course.Id, cancellationToken);
                }
                var courseCompletionRate = courseTotalAll > 0
                    ? Math.Round((double)courseCompletedAll / courseTotalAll * 100, 1)
                    : 0;

                var courseRevenue = allEnrollments
                    .Where(e => e.CourseId == course.Id && e.Status == EnrollmentStatus.Active)
                    .Sum(e => e.AmountPaid);

                coursePerformance.Add(new CoursePerformanceItem
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    EnrollmentCount = courseEnrollments,
                    AverageRating = courseAvgRating,
                    CompletionRate = courseCompletionRate,
                    Revenue = courseRevenue
                });
            }

            // Enrollment trend (last 6 months)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var enrollmentTrend = allEnrollments
                .Where(e => e.EnrolledAt >= sixMonthsAgo)
                .GroupBy(e => new { e.EnrolledAt.Year, e.EnrolledAt.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new EnrollmentTrendItem
                {
                    Month = $"{new DateTime(g.Key.Year, g.Key.Month, 1):MMM yyyy}",
                    Count = g.Count()
                })
                .ToList();

            return new TeacherDashboardStats
            {
                TotalCourses = teacherCourses.Count,
                PublishedCourses = teacherCourses.Count(c => c.IsPublished),
                DraftCourses = teacherCourses.Count(c => !c.IsPublished),
                TotalEnrollments = totalEnrollments,
                TotalStudents = totalStudents,
                TotalRevenue = totalRevenue,
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                TotalLectures = totalLectures,
                CompletionRate = completionRate,
                TotalExamsCreated = totalExams,
                PendingGradeApprovals = pendingApprovals,
                UngradedSubmissions = ungradedSubmissions,
                RecentEnrollments = recentEnrollments,
                CoursePerformance = coursePerformance,
                EnrollmentTrend = enrollmentTrend
            };
        }
    }
}
