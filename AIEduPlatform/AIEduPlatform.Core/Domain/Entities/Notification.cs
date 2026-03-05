namespace AIEduPlatform.Core.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string? RelatedEntityType { get; set; }
        public string? Metadata { get; set; }
        public DateTime? ReadAt { get; set; }

        public User User { get; set; } = null!;
    }
}
