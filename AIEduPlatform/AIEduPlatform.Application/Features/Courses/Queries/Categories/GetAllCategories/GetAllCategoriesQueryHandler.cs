using AIEduPlatform.Core.DTOs.Categories;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Categories.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = string.IsNullOrWhiteSpace(request.SearchTerm)
                ? (await _unitOfWork.Categories.GetAllAsync(cancellationToken)).ToList()
                : await _unitOfWork.Categories.SearchCategoriesAsync(request.SearchTerm, cancellationToken);

            return categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CourseCount = c.CourseCategories?.Count ?? 0,
                CreatedAt = c.CreatedAt
            }).ToList();
        }
    }
}
