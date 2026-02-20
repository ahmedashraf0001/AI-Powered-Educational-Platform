using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.SendEngagementAlerts
{
    /// <summary>
    /// Teacher sends engagement alerts to students whose engagement is below a threshold.
    /// </summary>
    public record SendEngagementAlertsCommand(
        Guid CourseId,
        /// <summary>Optional: specific student IDs to alert. If empty, alerts all at-risk students.</summary>
        List<Guid>? StudentIds = null,
        /// <summary>Optional custom message from the teacher.</summary>
        string? CustomMessage = null
    ) : IRequest<SendEngagementAlertsResult>;

    public class SendEngagementAlertsResult
    {
        public int AlertsSent { get; set; }
        public List<string> AlertedStudents { get; set; } = [];
    }
}
