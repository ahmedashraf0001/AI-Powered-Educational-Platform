using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Reviews.Commands.AddReview
{
    public class AddReviewCommandHandler : IRequestHandler<AddReviewCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AddReviewCommandHandler> _logger;

        public AddReviewCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<AddReviewCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Guid> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to review a course.");

            // Must be enrolled to review
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (!isEnrolled)
                throw new ForbiddenException("You must be enrolled in this course to leave a review.");

            // Check if already reviewed
            var existingReview = await _unitOfWork.Reviews.HasStudentReviewedAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (existingReview)
                throw new ConflictException("You have already reviewed this course. Use the update endpoint to modify your review.");

            var review = new Review
            {
                Id = Guid.CreateVersion7(),
                CourseId = request.CourseId,
                StudentId = userId.Value,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Review added. ReviewId: {ReviewId}, CourseId: {CourseId}, UserId: {UserId}, Rating: {Rating}",
                review.Id, request.CourseId, userId.Value, request.Rating);

            // Notify teacher about the new review
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);
            if (course != null)
            {
                await _notificationService.NotifyNewReviewAsync(
                    course.TeacherId,
                    course.Title,
                    request.Rating,
                    cancellationToken);
            }

            return review.Id;
        }
    }
}
