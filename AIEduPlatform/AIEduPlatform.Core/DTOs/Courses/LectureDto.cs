namespace AIEduPlatform.Core.DTOs.Courses
{
    public record LectureDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public List<MaterialDto> Materials { get; init; } = [];
    }
}
