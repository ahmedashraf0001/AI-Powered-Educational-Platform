using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UpdateMaterialProgress
{
    public record UpdateMaterialProgressCommand : IRequest<Unit>
    {
        public Guid MaterialId { get; init; }
        public int Position { get; init; }
    }
}
