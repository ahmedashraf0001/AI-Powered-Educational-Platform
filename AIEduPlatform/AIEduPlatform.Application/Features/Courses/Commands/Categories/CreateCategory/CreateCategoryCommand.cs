using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.CreateCategory
{
    public record CreateCategoryCommand : IRequest<Guid>
    {
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
    }
}
