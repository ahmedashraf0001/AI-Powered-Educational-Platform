namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record FlashcardDto
    {
        public Guid Id { get; init; }
        public string Topic { get; init; } = string.Empty;
        public string FrontText { get; init; } = string.Empty;
        public string BackText { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}
