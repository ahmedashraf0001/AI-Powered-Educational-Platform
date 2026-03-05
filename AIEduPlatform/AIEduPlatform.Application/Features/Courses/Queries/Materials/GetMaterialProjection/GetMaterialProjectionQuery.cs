using AIEduPlatform.Core.DTOs.Materials;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Materials.GetMaterialProjection
{
    public record GetMaterialProjectionQuery : IRequest<MaterialProjectionDto>
    {
        public Guid MaterialId { get; init; }
    }
}
