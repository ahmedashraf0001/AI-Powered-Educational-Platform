using AIEduPlatform.Core.DTOs.Common;

namespace AIEduPlatform.Core.DTOs.Notifications
{
    public record NotificationListDto
    {
        public List<NotificationDto> Items { get; init; } = [];
        public int UnreadCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalCount { get; init; }
    }
}
