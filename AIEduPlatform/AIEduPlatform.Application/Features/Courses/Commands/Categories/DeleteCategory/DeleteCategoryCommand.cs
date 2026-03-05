using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.DeleteCategory
{
    public record DeleteCategoryCommand : IRequest<Unit>
    {
        public Guid CategoryId { get; init; }
    }
}
