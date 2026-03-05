namespace AIEduPlatform.Core.DTOs.Categories
{
    public record CategoryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public int CourseCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
