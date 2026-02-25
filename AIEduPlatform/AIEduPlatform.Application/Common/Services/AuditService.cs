using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Common.Services
{
    /// <summary>
    /// Audit service implementation using structured logging.
    /// Audit events are written to a dedicated "Audit" logger category,
    /// allowing them to be routed to a separate sink (Seq, ELK, file, DB)
    /// via the logging configuration without coupling to a specific storage mechanism.
    /// </summary>
    public class AuditService : IAuditService
    {
        private readonly ILogger<AuditService> _logger;

        public AuditService(ILogger<AuditService> logger)
        {
            _logger = logger;
        }

        public Task LogGradeActionAsync(
            Guid performedByUserId,
            string action,
            Guid submissionId,
            Guid? gradeId = null,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[AUDIT] Grade | Action: {Action} | PerformedBy: {UserId} | SubmissionId: {SubmissionId} | GradeId: {GradeId} | Details: {Details}",
                action, performedByUserId, submissionId, gradeId, details);

            return Task.CompletedTask;
        }

        public Task LogCourseActionAsync(
            Guid performedByUserId,
            string action,
            Guid courseId,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[AUDIT] Course | Action: {Action} | PerformedBy: {UserId} | CourseId: {CourseId} | Details: {Details}",
                action, performedByUserId, courseId, details);

            return Task.CompletedTask;
        }

        public Task LogEnrollmentActionAsync(
            Guid performedByUserId,
            string action,
            Guid courseId,
            Guid? studentId = null,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[AUDIT] Enrollment | Action: {Action} | PerformedBy: {UserId} | CourseId: {CourseId} | StudentId: {StudentId} | Details: {Details}",
                action, performedByUserId, courseId, studentId, details);

            return Task.CompletedTask;
        }

        public Task LogAuthActionAsync(
            string action,
            string email,
            bool success,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[AUDIT] Auth | Action: {Action} | Email: {Email} | Success: {Success} | Details: {Details}",
                action, email, success, details);

            return Task.CompletedTask;
        }

        public Task LogAsync(
            string category,
            string action,
            Guid? performedByUserId = null,
            string? entityType = null,
            Guid? entityId = null,
            string? details = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "[AUDIT] {Category} | Action: {Action} | PerformedBy: {UserId} | EntityType: {EntityType} | EntityId: {EntityId} | Details: {Details}",
                category, action, performedByUserId, entityType, entityId, details);

            return Task.CompletedTask;
        }
    }
}
