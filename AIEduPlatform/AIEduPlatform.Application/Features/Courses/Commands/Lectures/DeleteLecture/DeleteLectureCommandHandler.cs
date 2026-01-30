using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture
{
    public class DeleteLectureCommandHandler : IRequestHandler<DeleteLectureCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteLectureCommandHandler> _logger;

        public DeleteLectureCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteLectureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to delete a lecture.");
            }

            _logger.LogInformation(
                "Deleting lecture. LectureId: {LectureId}, UserId: {UserId}",
                request.LectureId,
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
                        "User {UserId} is not authorized to delete lecture {LectureId}",
                        userId.Value,
                        request.LectureId);
                    throw new ForbiddenException("You are not authorized to delete this lecture.");
                }

                _logger.LogInformation(
                    "Found lecture to delete. LectureId: {LectureId}, Title: {Title}, CourseId: {CourseId}",
                    lecture.Id,
                    lecture.Title,
                    lecture.CourseId);

                await _unitOfWork.Lectures.DeleteAsync(lecture, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully deleted lecture. LectureId: {LectureId}, Title: {Title}",
                    request.LectureId,
                    lecture.Title);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error deleting lecture. LectureId: {LectureId}", request.LectureId);
                throw;
            }
        }
    }
}
