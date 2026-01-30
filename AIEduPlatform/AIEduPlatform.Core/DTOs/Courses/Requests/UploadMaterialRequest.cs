using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Courses.Requests
{
    public record UploadMaterialRequest
    {
        public string Title { get; init; } = string.Empty;
        public MaterialType Type { get; init; }
        public string? FileUrl { get; init; }
    }
}
