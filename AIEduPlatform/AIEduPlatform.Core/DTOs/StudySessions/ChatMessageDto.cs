namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record ChatMessageDto
    {
        public Guid Id { get; init; }
        public string Role { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public string? Sources { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
