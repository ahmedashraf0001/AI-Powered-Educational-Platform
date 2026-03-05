using AIEduPlatform.Core.DTOs.Categories;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Categories.GetCategoryById
{
    public record GetCategoryByIdQuery : IRequest<CategoryDto>
    {
        public Guid CategoryId { get; init; }
    }
}
