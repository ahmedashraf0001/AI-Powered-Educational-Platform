using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public record UploadMaterialCommand : IRequest<Guid>
    {
        public Guid LectureId { get; init; }
        public MaterialType Type { get; init; }
        public string Title { get; init; } = string.Empty;

        // Either provide FileUrl directly or provide file stream for upload
        public string? FileUrl { get; init; }

        // File upload properties
        public Stream? FileStream { get; init; }
        public string? FileName { get; init; }
        public string? ContentType { get; init; }
    }
}
