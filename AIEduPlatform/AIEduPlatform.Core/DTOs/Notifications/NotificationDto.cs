namespace AIEduPlatform.Core.DTOs.Notifications
{
    public record NotificationDto
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool IsRead { get; init; }
        public Guid? RelatedEntityId { get; init; }
        public string? RelatedEntityType { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? ReadAt { get; init; }
    }
}
