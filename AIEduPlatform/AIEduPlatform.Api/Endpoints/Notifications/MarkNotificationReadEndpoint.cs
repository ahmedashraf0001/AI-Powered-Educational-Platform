using AIEduPlatform.Application.Features.Courses.Commands.Notifications.MarkNotificationRead;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class MarkNotificationReadRequest
{
    public Guid Id { get; set; }
}

public class MarkNotificationReadEndpoint : Endpoint<MarkNotificationReadRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public MarkNotificationReadEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/notifications/{Id}/read");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Mark notification as read";
            s.Description = "Marks a single notification as read.";
            s.Response<ApiResponse<object>>(200, "Notification marked as read");
            s.Response(401, "Not authenticated");
            s.Response(404, "Notification not found");
        });
    }

    public override async Task HandleAsync(MarkNotificationReadRequest req, CancellationToken ct)
    {
        await _mediator.Send(new MarkNotificationReadCommand { NotificationId = req.Id }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Notification marked as read."), ct);
    }
}
