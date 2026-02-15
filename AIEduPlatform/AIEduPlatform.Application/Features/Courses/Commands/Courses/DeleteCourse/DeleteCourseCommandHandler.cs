using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse
{
    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRAGService _ragService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteCourseCommandHandler> _logger;

        public DeleteCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IRAGService ragService,
            ICurrentUserService currentUserService,
            ILogger<DeleteCourseCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _ragService = ragService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to delete a course.");
            }

            _logger.LogInformation(
                "Deleting course. CourseId: {CourseId}, UserId: {UserId}",
                request.CourseId,
                userId.Value);

            try
            {
                var course = await _unitOfWork.Courses.GetCourseByIdAsync(request.CourseId, null, cancellationToken);

                if (course == null)
                {
                    _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                    throw new NotFoundException(nameof(Course), request.CourseId);
                }

                if (course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to delete course {CourseId}",
                        userId.Value,
                        request.CourseId);
                    throw new ForbiddenException("You are not authorized to delete this course.");
                }

                _logger.LogInformation(
                    "Found course to delete. CourseId: {CourseId}, Title: {Title}",
                    course.Id,
                    course.Title);

                // RAG service deletes both the course and its chunks
                var ragDeleteResult = await _ragService.DeleteCourseAsync(request.CourseId, cancellationToken);
                
                if (!ragDeleteResult.Success)
                {
                    _logger.LogError(
                        "Failed to delete course {CourseId}: {Error}",
                        request.CourseId,
                        ragDeleteResult.Error);
                    throw new InvalidOperationException($"Failed to delete course: {ragDeleteResult.Error}");
                }

                _logger.LogInformation(
                    "Successfully deleted course. CourseId: {CourseId}, Title: {Title}",
                    request.CourseId,
                    course.Title);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error deleting course. CourseId: {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
