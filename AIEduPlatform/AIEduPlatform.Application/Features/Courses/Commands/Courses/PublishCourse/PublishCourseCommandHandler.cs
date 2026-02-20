using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse
{
    public class PublishCourseCommandHandler : IRequestHandler<PublishCourseCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PublishCourseCommandHandler> _logger;

        public PublishCourseCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<PublishCourseCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(PublishCourseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to publish a course.");
            }

            _logger.LogInformation(
                "Publishing course. CourseId: {CourseId}, IsPublished: {IsPublished}, UserId: {UserId}",
                request.CourseId,
                request.IsPublished,
                userId.Value);

            try
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

                if (course == null)
                {
                    _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                    throw new NotFoundException(nameof(Course), request.CourseId);
                }

                if (course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to publish course {CourseId}",
                        userId.Value,
                        request.CourseId);
                    throw new ForbiddenException("You are not authorized to publish this course.");
                }

                course.IsPublished = request.IsPublished;
                course.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully updated course publish status. CourseId: {CourseId}, IsPublished: {IsPublished}",
                    request.CourseId,
                    request.IsPublished);

                // Notify students about publish/unpublish
                await _notificationService.NotifyCoursePublishedAsync(
                    request.CourseId,
                    course.Title,
                    request.IsPublished,
                    cancellationToken);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error publishing course. CourseId: {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
