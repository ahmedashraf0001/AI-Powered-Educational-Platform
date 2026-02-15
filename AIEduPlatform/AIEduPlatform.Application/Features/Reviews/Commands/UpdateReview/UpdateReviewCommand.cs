using MediatR;

namespace AIEduPlatform.Application.Features.Reviews.Commands.UpdateReview
{
    public record UpdateReviewCommand : IRequest<Unit>
    {
        public Guid ReviewId { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
    }
}
