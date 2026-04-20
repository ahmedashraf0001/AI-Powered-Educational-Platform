using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.ReindexMaterial;

public class ReindexMaterialCommand : IRequest<Unit>
{
    public Guid MaterialId { get; set; }
}
