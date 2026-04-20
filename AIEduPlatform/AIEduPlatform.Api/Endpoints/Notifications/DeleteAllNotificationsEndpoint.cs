using AIEduPlatform.Application.Features.Courses.Commands.Notifications.DeleteAllNotifications;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class DeleteAllNotificationsEndpoint : EndpointWithoutRequest<ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public DeleteAllNotificationsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/notifications");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete all notifications";
            s.Description = "Deletes all notifications for the current user.";
            s.Response<ApiResponse<object>>(200, "All notifications deleted");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _mediator.Send(new DeleteAllNotificationsCommand(), ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "All notifications deleted."), ct);
    }
}
