using AIEduPlatform.Core.DTOs.Notifications;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Notifications.GetNotifications
{
    public record GetNotificationsQuery : IRequest<NotificationListDto>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public bool UnreadOnly { get; init; }
    }
}
