using AIEduPlatform.Core.DTOs.Categories;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Categories.GetAllCategories
{
    public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>
    {
        public string? SearchTerm { get; init; }
    }
}
