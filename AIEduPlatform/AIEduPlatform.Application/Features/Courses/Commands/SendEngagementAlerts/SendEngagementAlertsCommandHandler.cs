using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.SendEngagementAlerts
{
    public class SendEngagementAlertsCommandHandler
        : IRequestHandler<SendEngagementAlertsCommand, SendEngagementAlertsResult>
    {
        private readonly IUnitOfWork _uow;
        private readonly ICurrentUserService _currentUser;
        private readonly INotificationService _notifications;
        private readonly ILogger<SendEngagementAlertsCommandHandler> _logger;

        public SendEngagementAlertsCommandHandler(
            IUnitOfWork uow,
            ICurrentUserService currentUser,
            INotificationService notifications,
            ILogger<SendEngagementAlertsCommandHandler> logger)
        {
            _uow = uow;
            _currentUser = currentUser;
            _notifications = notifications;
            _logger = logger;
        }

        public async Task<SendEngagementAlertsResult> Handle(
            SendEngagementAlertsCommand request,
            CancellationToken ct)
        {
            var teacherId = _currentUser.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var course = await _uow.Courses.GetByIdAsync(request.CourseId, ct)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            if (course.TeacherId != teacherId)
                throw new ForbiddenException("You are not the teacher of this course.");

            // Get enrolled students
            var enrollments = await _uow.Enrollments
                .GetEnrollmentsByCourseAsync(request.CourseId, includeStudent: true, ct: ct);

            // Filter to specific students if provided
            var targets = request.StudentIds is { Count: > 0 }
                ? enrollments.Where(e => request.StudentIds.Contains(e.StudentId)).ToList()
                : enrollments;

            if (targets.Count == 0)
                throw new BadRequestException("No enrolled students match the provided criteria.");

            // Determine which students to alert
            var studentsToAlert = new List<(Guid StudentId, string Name, EngagementLevel Level)>();

            foreach (var enrollment in targets)
            {
                var sid = enrollment.StudentId;

                // Compute a quick engagement assessment
                var sessionStats = await _uow.StudySessions
                    .GetStudentStatsAsync(sid, request.CourseId, ct);

                var daysSinceLastActivity = sessionStats.LastSessionDate.HasValue
                    ? (int)(DateTime.UtcNow - sessionStats.LastSessionDate.Value).TotalDays
                    : (int)(DateTime.UtcNow - enrollment.EnrolledAt).TotalDays;

                var level = ClassifyEngagement(
                    sessionStats.TotalSessions,
                    sessionStats.TotalMessages + sessionStats.TotalFlashcards +
                    sessionStats.TotalQuizzes + sessionStats.TotalMindMaps,
                    daysSinceLastActivity);

                // If specific students were requested, alert all of them regardless of level.
                // Otherwise only alert Critical or Low engagement students.
                if (request.StudentIds is { Count: > 0 }
                    || level is EngagementLevel.Critical or EngagementLevel.Low)
                {
                    var name = $"{enrollment.Student?.FirstName} {enrollment.Student?.LastName}".Trim();
                    studentsToAlert.Add((sid, name, level));
                }
            }

            if (studentsToAlert.Count == 0)
            {
                return new SendEngagementAlertsResult
                {
                    AlertsSent = 0,
                    AlertedStudents = []
                };
            }

            // Send notifications
            var teacher = await _uow.Users.GetUserByIdAsync(teacherId, ct: ct);
            var teacherName = $"{teacher?.FirstName} {teacher?.LastName}".Trim();

            foreach (var (studentId, name, level) in studentsToAlert)
            {
                await _notifications.NotifyLowEngagementAlertAsync(
                    studentId,
                    course.Title,
                    teacherName,
                    level.ToString(),
                    request.CustomMessage,
                    ct);

                _logger.LogInformation(
                    "Engagement alert sent. StudentId: {StudentId}, Course: {CourseName}, Level: {Level}",
                    studentId, course.Title, level);
            }

            return new SendEngagementAlertsResult
            {
                AlertsSent = studentsToAlert.Count,
                AlertedStudents = studentsToAlert.Select(s => s.Name).ToList()
            };
        }

        private static EngagementLevel ClassifyEngagement(
            int sessions, int interactions, int daysSinceLastActivity)
        {
            var score = 0.0;
            score += Math.Min(sessions / 5.0, 1.0) * 40;
            score += Math.Min(interactions / 15.0, 1.0) * 30;
            score += daysSinceLastActivity switch
            {
                <= 3 => 30,
                <= 7 => 20,
                <= 14 => 10,
                _ => 0
            };

            return score switch
            {
                <= 25 => EngagementLevel.Critical,
                <= 50 => EngagementLevel.Low,
                <= 75 => EngagementLevel.Moderate,
                _ => EngagementLevel.High
            };
        }
    }
}
