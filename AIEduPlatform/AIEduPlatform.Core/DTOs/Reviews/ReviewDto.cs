namespace AIEduPlatform.Core.DTOs.Reviews
{
    public record ReviewDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public Guid StudentId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public int Rating { get; init; }
        public string? Comment { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
