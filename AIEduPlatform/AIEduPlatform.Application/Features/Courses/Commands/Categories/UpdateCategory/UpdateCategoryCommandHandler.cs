using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                ?? throw new NotFoundException(nameof(Category), request.CategoryId);

            var existing = await _unitOfWork.Categories.GetByNameAsync(request.Name, cancellationToken);
            if (existing != null && existing.Id != request.CategoryId)
                throw new ConflictException($"Category '{request.Name}' already exists.");

            category.Name = request.Name;
            category.Description = request.Description;

            await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
