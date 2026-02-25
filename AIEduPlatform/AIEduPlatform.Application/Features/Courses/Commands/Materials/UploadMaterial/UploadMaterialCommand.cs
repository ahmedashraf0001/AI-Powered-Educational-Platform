using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UploadMaterial
{
    public record UploadMaterialFile
    {
        public string Title { get; init; } = string.Empty;
        public Stream FileStream { get; init; } = null!;
        public string FileName { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
    }

    public record UploadMaterialCommand : IRequest<List<Guid>>
    {
        public Guid LectureId { get; init; }
        public List<UploadMaterialFile> Files { get; init; } = [];
    }
}
