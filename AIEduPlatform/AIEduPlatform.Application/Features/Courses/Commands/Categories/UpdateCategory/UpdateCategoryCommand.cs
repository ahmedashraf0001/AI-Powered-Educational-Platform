using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.UpdateCategory
{
    public record UpdateCategoryCommand : IRequest<Unit>
    {
        public Guid CategoryId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
