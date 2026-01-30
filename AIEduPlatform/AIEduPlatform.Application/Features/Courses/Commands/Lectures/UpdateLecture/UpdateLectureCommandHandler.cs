using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture
{
    public class UpdateLectureCommandHandler : IRequestHandler<UpdateLectureCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateLectureCommandHandler> _logger;

        public UpdateLectureCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateLectureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to update a lecture.");
            }

            _logger.LogInformation(
                "Updating lecture. LectureId: {LectureId}, Title: {Title}, UserId: {UserId}",
                request.LectureId,
                request.Title,
                userId.Value);

            try
            {
                var lecture = await _unitOfWork.Lectures.GetByIdAsync(request.LectureId, cancellationToken);

                if (lecture == null)
                {
                    _logger.LogWarning("Lecture not found. LectureId: {LectureId}", request.LectureId);
                    throw new NotFoundException(nameof(Lecture), request.LectureId);
                }

                var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);

                if (course == null || course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to update lecture {LectureId}",
                        userId.Value,
                        request.LectureId);
                    throw new ForbiddenException("You are not authorized to update this lecture.");
                }

                lecture.Title = request.Title;
                lecture.Description = request.Description;
                lecture.OrderIndex = request.OrderIndex;
                lecture.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Lectures.UpdateAsync(lecture, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully updated lecture. LectureId: {LectureId}, Title: {Title}",
                    lecture.Id,
                    lecture.Title);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error updating lecture. LectureId: {LectureId}", request.LectureId);
                throw;
            }
        }
    }
}
