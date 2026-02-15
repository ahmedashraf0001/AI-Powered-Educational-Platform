using MediatR;

namespace AIEduPlatform.Application.Features.Reviews.Commands.DeleteReview
{
    public record DeleteReviewCommand : IRequest<Unit>
    {
        public Guid ReviewId { get; init; }
    }
}
