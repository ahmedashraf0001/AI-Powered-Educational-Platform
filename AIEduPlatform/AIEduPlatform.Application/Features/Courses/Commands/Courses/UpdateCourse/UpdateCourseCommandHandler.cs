using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse
{
    public class UpdateCourseCommandHandler : IRequestHandler<UpdateCourseCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UpdateCourseCommandHandler> _logger;

        public UpdateCourseCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<UpdateCourseCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateCourseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to update a course.");
            }

            _logger.LogInformation(
                "Updating course. CourseId: {CourseId}, Title: {Title}, UserId: {UserId}",
                request.CourseId,
                request.Title,
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
                        "User {UserId} is not authorized to update course {CourseId}",
                        userId.Value,
                        request.CourseId);
                    throw new ForbiddenException("You are not authorized to update this course.");
                }

                course.Title = request.Title;
                course.Description = request.Description;
                course.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully updated course. CourseId: {CourseId}, Title: {Title}",
                    course.Id,
                    course.Title);

                // Notify students about course update
                await _notificationService.NotifyCourseUpdatedAsync(
                    request.CourseId,
                    course.Title,
                    cancellationToken);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error updating course. CourseId: {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}