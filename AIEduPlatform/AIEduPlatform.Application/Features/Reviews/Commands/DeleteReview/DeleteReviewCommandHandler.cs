using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Reviews.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteReviewCommandHandler> _logger;

        public DeleteReviewCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteReviewCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to delete a review.");

            var review = await _unitOfWork.Reviews.GetByIdAsync(request.ReviewId, cancellationToken);

            if (review == null)
                throw new NotFoundException(nameof(Review), request.ReviewId);

            // Only the review author or the course instructor can delete a review
            if (review.StudentId != userId.Value)
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(review.CourseId, cancellationToken);
                if (course == null || course.TeacherId != userId.Value)
                    throw new ForbiddenException("You can only delete your own reviews.");
            }

            await _unitOfWork.Reviews.DeleteAsync(review, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Review deleted. ReviewId: {ReviewId}, UserId: {UserId}",
                request.ReviewId, userId.Value);

            return Unit.Value;
        }
    }
}
