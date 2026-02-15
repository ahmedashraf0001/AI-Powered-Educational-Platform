using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Courses
{
    public record MaterialDto
    {
        public Guid Id { get; init; }
        public Guid LectureId { get; init; }
        public MaterialType Type { get; init; }
        public string Title { get; init; } = string.Empty;
        public string StreamUrl { get; init; } = string.Empty;
        public bool Indexed { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
    }
}
