using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Categories.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateCategoryCommandHandler> _logger;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateCategoryCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.Categories.GetByNameAsync(request.Name, cancellationToken);
            if (existing != null)
                throw new ConflictException($"Category '{request.Name}' already exists.");

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description
            };

            var created = await _unitOfWork.Categories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created category: {Name} (ID: {Id})", created.Name, created.Id);

            return created.Id;
        }
    }
}
