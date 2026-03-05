using AIEduPlatform.Application.Features.Courses.Commands.Notifications.DeleteNotification;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class DeleteNotificationRequest
{
    public Guid Id { get; set; }
}

public class DeleteNotificationEndpoint : Endpoint<DeleteNotificationRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public DeleteNotificationEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/notifications/{Id}");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete notification";
            s.Description = "Deletes a single notification.";
            s.Response<ApiResponse<object>>(200, "Notification deleted");
            s.Response(401, "Not authenticated");
            s.Response(404, "Notification not found");
        });
    }

    public override async Task HandleAsync(DeleteNotificationRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteNotificationCommand { NotificationId = req.Id }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Notification deleted."), ct);
    }
}
