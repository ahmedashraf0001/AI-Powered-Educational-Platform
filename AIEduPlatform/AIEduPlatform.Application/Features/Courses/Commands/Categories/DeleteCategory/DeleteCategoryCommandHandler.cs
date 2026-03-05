using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteCategoryCommandHandler> _logger;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<DeleteCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted category: {Name} (ID: {Id})", category.Name, category.Id);

            return Unit.Value;
        }
    }
}
