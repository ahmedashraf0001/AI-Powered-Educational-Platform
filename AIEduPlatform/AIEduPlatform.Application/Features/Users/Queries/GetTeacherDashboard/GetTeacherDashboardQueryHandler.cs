using AIEduPlatform.Application.Common.Exceptions;
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

            var teacherCourses = (await _uow.Courses.FindAsync(
                c => c.TeacherId == userId, cancellationToken)).ToList();

            var courseIds = teacherCourses.Select(c => c.Id).ToList();

            var totalStudents = await _uow.Enrollments.CountAsync(
                e => courseIds.Contains(e.CourseId), cancellationToken);

            var totalExams = await _uow.Exams.CountAsync(
                e => courseIds.Contains(e.CourseId), cancellationToken);

            var pendingApprovals = await _uow.Grades.CountAsync(
                g => g.IsAiGraded && !g.IsApproved
                    && courseIds.Contains(g.Submission.Exam.CourseId), cancellationToken);

            var ungradedSubmissions = await _uow.Submissions.CountAsync(
                s => s.Grade == null
                    && courseIds.Contains(s.Exam.CourseId), cancellationToken);

            return new TeacherDashboardStats
            {
                TotalCourses = teacherCourses.Count,
                PublishedCourses = teacherCourses.Count(c => c.IsPublished),
                TotalStudentsEnrolled = totalStudents,
                TotalExamsCreated = totalExams,
                PendingGradeApprovals = pendingApprovals,
                UngradedSubmissions = ungradedSubmissions
            };
        }
    }
}
