using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Materials.UpdateMaterialProgress
{
    public class UpdateMaterialProgressCommandHandler : IRequestHandler<UpdateMaterialProgressCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public UpdateMaterialProgressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(UpdateMaterialProgressCommand request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var material = await _unitOfWork.Materials.GetMaterialByIdAsync(request.MaterialId, ct: cancellationToken)
                ?? throw new NotFoundException(nameof(Material), request.MaterialId);

            // Validate enrollment
            var lecture = await _unitOfWork.Lectures.GetByIdAsync(material.LectureId, cancellationToken)
                ?? throw new NotFoundException(nameof(Lecture), material.LectureId);

            if (!await _unitOfWork.Enrollments.IsStudentEnrolledAsync(studentId, lecture.CourseId, cancellationToken))
                throw new BadRequestException("You are not enrolled in this course.");

            var progress = await _unitOfWork.MaterialProgress.GetProgressAsync(studentId, request.MaterialId, cancellationToken);

            if (progress == null)
            {
                // Create new progress record
                progress = new MaterialProgress
                {
                    StudentId = studentId,
                    MaterialId = request.MaterialId,
                    LastPosition = request.Position,
                    IsCompleted = IsCompleted(material, request.Position)
                };

                await _unitOfWork.MaterialProgress.AddAsync(progress, cancellationToken);
            }
            else
            {
                // Conflict-safe update: only overwrite if incoming value is strictly greater
                if (request.Position > progress.LastPosition)
                {
                    progress.LastPosition = request.Position;
                    progress.UpdatedAt = DateTime.UtcNow;
                }

                // Check completion (can only go from false to true, never back)
                if (!progress.IsCompleted)
                    progress.IsCompleted = IsCompleted(material, progress.LastPosition);

                await _unitOfWork.MaterialProgress.UpdateAsync(progress, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }

        /// <summary>
        /// Completion thresholds per spec:
        /// - Video/Audio: >= 95% of duration
        /// - PDF/Document: Last page reached (position >= totalPages)
        /// </summary>
        private static bool IsCompleted(Material material, int position)
        {
            return material.Type switch
            {
                MaterialType.Video or MaterialType.Audio =>
                    material.DurationSeconds.HasValue && material.DurationSeconds.Value > 0
                    && (double)position / material.DurationSeconds.Value >= 0.95,

                MaterialType.Document =>
                    material.TotalPages.HasValue && material.TotalPages.Value > 0
                    && position >= material.TotalPages.Value,

                _ => false
            };
        }
    }
}
