using AIEduPlatform.Application.Features.Courses.Commands.SendEngagementAlerts;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class SendEngagementAlertsRequest
{
    public Guid CourseId { get; set; }
    public List<Guid>? StudentIds { get; set; }
    public string? CustomMessage { get; set; }
}

public class SendEngagementAlertsEndpoint
    : Endpoint<SendEngagementAlertsRequest, ApiResponse<SendEngagementAlertsResult>>
{
    private readonly IMediator _mediator;

    public SendEngagementAlertsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/engagement/alerts");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Send engagement alerts to at-risk students";
            s.Description = "Sends a real-time notification to students with low engagement in the course. " +
                            "If no StudentIds are provided, all students with Critical or Low engagement " +
                            "will be alerted. A custom message can be included.";
            s.ExampleRequest = new SendEngagementAlertsRequest
            {
                CourseId = Guid.Empty,
                StudentIds = null,
                CustomMessage = "Please catch up on the recent lectures and complete the pending assignments."
            };
            s.Response<ApiResponse<SendEngagementAlertsResult>>(200, "Alerts sent");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course teacher");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(SendEngagementAlertsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(
            new SendEngagementAlertsCommand(req.CourseId, req.StudentIds, req.CustomMessage), ct);
        await SendOkAsync(ApiResponse<SendEngagementAlertsResult>.Ok(result), ct);
    }
}
