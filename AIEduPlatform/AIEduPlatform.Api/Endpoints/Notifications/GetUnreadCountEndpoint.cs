using AIEduPlatform.Application.Features.Courses.Queries.Notifications.GetUnreadCount;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class GetUnreadCountResponse
{
    public int Count { get; set; }
}

public class GetUnreadCountEndpoint : EndpointWithoutRequest<ApiResponse<GetUnreadCountResponse>>
{
    private readonly IMediator _mediator;

    public GetUnreadCountEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/notifications/unread-count");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Get unread notification count";
            s.Description = "Returns the number of unread notifications for the current user.";
            s.Response<ApiResponse<GetUnreadCountResponse>>(200, "Unread count retrieved");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var count = await _mediator.Send(new GetUnreadCountQuery(), ct);
        await SendOkAsync(ApiResponse<GetUnreadCountResponse>.Ok(new GetUnreadCountResponse { Count = count }), ct);
    }
}
