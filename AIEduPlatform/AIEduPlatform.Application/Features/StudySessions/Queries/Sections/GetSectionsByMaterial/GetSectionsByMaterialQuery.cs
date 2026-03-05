using AIEduPlatform.Core.DTOs.Materials;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sections.GetSectionsByMaterial
{
    public record GetSectionsByMaterialQuery : IRequest<List<SemanticSectionDto>>
    {
        public Guid MaterialId { get; init; }
    }
}
