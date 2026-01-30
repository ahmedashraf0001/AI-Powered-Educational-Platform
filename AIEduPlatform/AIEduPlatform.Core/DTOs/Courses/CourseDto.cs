namespace AIEduPlatform.Core.DTOs.Courses
{
    public record CourseDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid TeacherId { get; init; }
        public bool IsPublished { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
