using MediatR;

namespace AIEduPlatform.Application.Features.Reviews.Commands.AddReview
{
    public record AddReviewCommand : IRequest<Guid>
    {
        public Guid CourseId { get; init; }
        public int Rating { get; init; }
        public string? Comment { get; init; }
    }
}
