using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Courses
{
    public record LectureDetailDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public Dictionary<string, List<MaterialDto>> MaterialsByType { get; init; } = new();
        public int TotalMaterials { get; init; }
    }
}
