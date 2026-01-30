using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.DeleteMaterial
{
    public class DeleteMaterialCommandHandler : IRequestHandler<DeleteMaterialCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteMaterialCommandHandler> _logger;

        public DeleteMaterialCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteMaterialCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteMaterialCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to delete materials.");
            }

            _logger.LogInformation(
                "Deleting material. MaterialId: {MaterialId}, UserId: {UserId}",
                request.MaterialId,
                userId.Value);

            try
            {
                var material = await _unitOfWork.Materials.GetByIdAsync(request.MaterialId, cancellationToken);

                if (material == null)
                {
                    _logger.LogWarning("Material not found. MaterialId: {MaterialId}", request.MaterialId);
                    throw new NotFoundException(nameof(Material), request.MaterialId);
                }

                var lecture = await _unitOfWork.Lectures.GetByIdAsync(material.LectureId, cancellationToken);

                if (lecture == null)
                {
                    throw new NotFoundException(nameof(Lecture), material.LectureId);
                }

                var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);

                if (course == null || course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to delete material {MaterialId}",
                        userId.Value,
                        request.MaterialId);
                    throw new ForbiddenException("You are not authorized to delete this material.");
                }

                _logger.LogInformation(
                    "Found material to delete. MaterialId: {MaterialId}, Title: {Title}, LectureId: {LectureId}",
                    material.Id,
                    material.Title,
                    material.LectureId);

                await _unitOfWork.Materials.DeleteAsync(material, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully deleted material. MaterialId: {MaterialId}, Title: {Title}",
                    request.MaterialId,
                    material.Title);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error deleting material. MaterialId: {MaterialId}", request.MaterialId);
                throw;
            }
        }
    }
}
