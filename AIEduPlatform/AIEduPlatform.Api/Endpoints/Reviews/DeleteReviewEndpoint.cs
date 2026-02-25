using AIEduPlatform.Application.Features.Reviews.Commands.DeleteReview;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public class DeleteReviewRequest
{
    public Guid ReviewId { get; set; }
}

public class DeleteReviewEndpoint : Endpoint<DeleteReviewRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public DeleteReviewEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/reviews/{ReviewId}");
        Group<ReviewsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a review";
            s.Description = "Delete a review. The review author or the course instructor can delete it.";
            s.Response<ApiResponse<object>>(200, "Review deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not authorized to delete this review");
            s.Response(404, "Review not found");
        });
    }

    public override async Task HandleAsync(DeleteReviewRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteReviewCommand
        {
            ReviewId = req.ReviewId
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Review deleted successfully."), ct);
    }
}
