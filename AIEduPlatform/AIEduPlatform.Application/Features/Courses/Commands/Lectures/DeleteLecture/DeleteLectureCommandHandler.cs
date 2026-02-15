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
        private readonly IRAGService _ragService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteLectureCommandHandler> _logger;

        public DeleteLectureCommandHandler(
            IUnitOfWork unitOfWork,
            IRAGService ragService,
            ICurrentUserService currentUserService,
            ILogger<DeleteLectureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _ragService = ragService;
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

                // RAG service deletes both the lecture and its chunks
                var ragDeleteResult = await _ragService.DeleteLectureAsync(request.LectureId, cancellationToken);
                
                if (!ragDeleteResult.Success)
                {
                    _logger.LogError(
                        "Failed to delete lecture {LectureId}: {Error}",
                        request.LectureId,
                        ragDeleteResult.Error);
                    throw new InvalidOperationException($"Failed to delete lecture: {ragDeleteResult.Error}");
                }

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
