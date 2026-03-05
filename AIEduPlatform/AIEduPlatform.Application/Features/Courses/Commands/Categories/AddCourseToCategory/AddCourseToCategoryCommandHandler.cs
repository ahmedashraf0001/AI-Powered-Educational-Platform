using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.AddCourseToCategory
{
    public class AddCourseToCategoryCommandHandler : IRequestHandler<AddCourseToCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AddCourseToCategoryCommandHandler> _logger;

        public AddCourseToCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<AddCourseToCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(AddCourseToCategoryCommand request, CancellationToken cancellationToken)
        {
            if (!await _unitOfWork.Courses.CourseExistsAsync(request.CourseId, cancellationToken))
                throw new NotFoundException(nameof(Course), request.CourseId);

            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            // Check if association already exists
            var courseCategories = await _unitOfWork.Categories.GetCategoriesByCourseAsync(request.CourseId, cancellationToken);
            if (courseCategories.Any(c => c.Id == request.CategoryId))
                throw new ConflictException($"Course is already associated with category '{category.Name}'.");

            var courseCategory = new CourseCategory
            {
                CourseId = request.CourseId,
                CategoryId = request.CategoryId
            };

            await _unitOfWork.CourseCategories.AddAsync(courseCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added course {CourseId} to category {CategoryId}", request.CourseId, request.CategoryId);

            return Unit.Value;
        }
    }
}
