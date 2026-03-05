using AIEduPlatform.Application.Features.Courses.Queries.Notifications.GetNotifications;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Notifications;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Notifications;

public class GetNotificationsRequest
{
    [QueryParam]
    public int Page { get; set; } = 1;

    [QueryParam]
    public int PageSize { get; set; } = 20;

    [QueryParam]
    public bool UnreadOnly { get; set; }
}

public class GetNotificationsEndpoint : Endpoint<GetNotificationsRequest, ApiResponse<NotificationListDto>>
{
    private readonly IMediator _mediator;

    public GetNotificationsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/notifications");
        Group<NotificationsGroup>();
        Summary(s =>
        {
            s.Summary = "Get notifications";
            s.Description = "Returns paginated notifications for the current user.";
            s.Response<ApiResponse<NotificationListDto>>(200, "Notifications retrieved");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetNotificationsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetNotificationsQuery
        {
            Page = req.Page,
            PageSize = req.PageSize,
            UnreadOnly = req.UnreadOnly
        }, ct);

        await SendOkAsync(ApiResponse<NotificationListDto>.Ok(result), ct);
    }
}
