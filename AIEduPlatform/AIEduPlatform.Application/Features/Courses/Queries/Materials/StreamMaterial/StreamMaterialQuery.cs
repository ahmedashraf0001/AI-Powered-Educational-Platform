using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Materials.StreamMaterial
{
    public record StreamMaterialQuery : IRequest<StreamMaterialResult>
    {
        public Guid MaterialId { get; init; }
    }

    public record StreamMaterialResult
    {
        public string FilePath { get; init; } = string.Empty;
        public string ContentType { get; init; } = string.Empty;
        public string FileName { get; init; } = string.Empty;
        public MaterialType Type { get; init; }
    }
}
