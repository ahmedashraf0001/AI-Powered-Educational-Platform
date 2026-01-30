using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.DeleteMaterial
{
    public record DeleteMaterialCommand : IRequest<Unit>
    {
        public Guid MaterialId { get; init; }
    }
}
