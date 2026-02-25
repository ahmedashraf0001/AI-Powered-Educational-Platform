namespace AIEduPlatform.Core.Interfaces.Services
{
    /// <summary>
    /// Service for recording audit trail events for sensitive operations.
    /// Uses structured logging so events can be routed to a dedicated audit sink
    /// (e.g., Seq, ELK, Application Insights) via the logging pipeline.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Record an audit event for a grade-related action
        /// </summary>
        Task LogGradeActionAsync(
            Guid performedByUserId,
            string action,
            Guid submissionId,
            Guid? gradeId = null,
            string? details = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Record an audit event for a course-related action
        /// </summary>
        Task LogCourseActionAsync(
            Guid performedByUserId,
            string action,
            Guid courseId,
            string? details = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Record an audit event for an enrollment-related action
        /// </summary>
        Task LogEnrollmentActionAsync(
            Guid performedByUserId,
            string action,
            Guid courseId,
            Guid? studentId = null,
            string? details = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Record an audit event for an authentication-related action
        /// </summary>
        Task LogAuthActionAsync(
            string action,
            string email,
            bool success,
            string? details = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Record a generic audit event
        /// </summary>
        Task LogAsync(
            string category,
            string action,
            Guid? performedByUserId = null,
            string? entityType = null,
            Guid? entityId = null,
            string? details = null,
            CancellationToken cancellationToken = default);
    }
}
