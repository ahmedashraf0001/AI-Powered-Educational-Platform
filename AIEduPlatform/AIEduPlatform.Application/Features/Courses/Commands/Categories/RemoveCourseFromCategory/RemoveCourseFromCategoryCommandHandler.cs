using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.RemoveCourseFromCategory
{
    public class RemoveCourseFromCategoryCommandHandler : IRequestHandler<RemoveCourseFromCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RemoveCourseFromCategoryCommandHandler> _logger;

        public RemoveCourseFromCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<RemoveCourseFromCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(RemoveCourseFromCategoryCommand request, CancellationToken cancellationToken)
        {
            var associations = await _unitOfWork.CourseCategories.FindAsync(
                cc => cc.CourseId == request.CourseId && cc.CategoryId == request.CategoryId,
                cancellationToken);

            var courseCategory = associations.FirstOrDefault()
                ?? throw new NotFoundException($"Course-Category association not found for CourseId '{request.CourseId}' and CategoryId '{request.CategoryId}'.");

            await _unitOfWork.CourseCategories.DeleteAsync(courseCategory, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed course {CourseId} from category {CategoryId}", request.CourseId, request.CategoryId);

            return Unit.Value;
        }
    }
}
