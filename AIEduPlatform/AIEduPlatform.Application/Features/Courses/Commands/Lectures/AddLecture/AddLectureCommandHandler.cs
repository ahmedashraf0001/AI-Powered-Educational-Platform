using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.AddLecture
{
    public class AddLectureCommandHandler : IRequestHandler<AddLectureCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AddLectureCommandHandler> _logger;

        public AddLectureCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<AddLectureCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Guid> Handle(AddLectureCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to add a lecture.");
            }

            _logger.LogInformation(
                "Adding lecture to course. CourseId: {CourseId}, Title: {Title}, UserId: {UserId}",
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
                        "User {UserId} is not authorized to add lectures to course {CourseId}",
                        userId.Value,
                        request.CourseId);
                    throw new ForbiddenException("You are not authorized to add lectures to this course.");
                }

                var lecture = new Lecture
                {
                    CourseId = request.CourseId,
                    Title = request.Title,
                    Description = request.Description,
                    OrderIndex = request.OrderIndex,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdLecture = await _unitOfWork.Lectures.AddAsync(lecture, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully added lecture. LectureId: {LectureId}, CourseId: {CourseId}, Title: {Title}",
                    createdLecture.Id,
                    request.CourseId,
                    lecture.Title);

                // Notify students about new lecture
                await _notificationService.NotifyNewLectureAddedAsync(
                    request.CourseId,
                    course.Title,
                    request.Title,
                    cancellationToken);

                return createdLecture.Id;
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error adding lecture to course. CourseId: {CourseId}", request.CourseId);
                throw;
            }
        }
    }
}
