using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Categories;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Categories.GetCategoryById
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                CourseCount = category.CourseCategories?.Count ?? 0,
                CreatedAt = category.CreatedAt
            };
        }
    }
}
