using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Materials
{
    public record MaterialProjectionDto
    {
        public Guid LessonId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string MaterialType { get; init; } = string.Empty;
        public string MaterialUrl { get; init; } = string.Empty;
        public MaterialProgressDto Progress { get; init; } = new();
        public bool IsCompleted { get; init; }
        public SemanticSectionDto? CurrentSection { get; init; }
    }

    public record MaterialProgressDto
    {
        public int Current { get; init; }
        public int Total { get; init; }
        public double Percentage { get; init; }
    }

    public record SemanticSectionDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Summary { get; init; } = string.Empty;
        public int? StartSeconds { get; init; }
        public int? EndSeconds { get; init; }
        public int? StartPage { get; init; }
        public int? EndPage { get; init; }
        public int OrderIndex { get; init; }
    }
}
