namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record SessionDetailDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime LastActivity { get; init; }
        public int MessageCount { get; init; }
        public int FlashcardCount { get; init; }
        public int QuizCount { get; init; }
        public int MindMapCount { get; init; }
    }
}
