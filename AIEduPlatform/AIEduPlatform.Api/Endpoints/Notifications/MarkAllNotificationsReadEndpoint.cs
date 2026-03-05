using AIEduPlatform.Application.Features.Courses.Commands.Notifications.MarkAllNotificationsRead;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class MarkAllNotificationsReadEndpoint : EndpointWithoutRequest<ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public MarkAllNotificationsReadEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/notifications/read-all");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Mark all notifications as read";
            s.Description = "Marks all notifications for the current user as read.";
            s.Response<ApiResponse<object>>(200, "All notifications marked as read");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _mediator.Send(new MarkAllNotificationsReadCommand(), ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "All notifications marked as read."), ct);
    }
}
