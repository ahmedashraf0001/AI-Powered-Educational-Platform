using AIEduPlatform.Application.Features.Reviews.Commands.UpdateReview;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public class UpdateReviewRequest
{
    public Guid ReviewId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewEndpoint : Endpoint<UpdateReviewRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public UpdateReviewEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/reviews/{ReviewId}");
        Roles("Student");
        Group<ReviewsGroup>();
        Summary(s =>
        {
            s.Summary = "Update a review";
            s.Description = "Update your review and rating. Only the review author can update it.";
            s.ExampleRequest = new UpdateReviewRequest
            {
                ReviewId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Rating = 4,
                Comment = "Updated review: Great content but could use more practical examples."
            };
            s.Response<ApiResponse<object>>(200, "Review updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the review author");
            s.Response(404, "Review not found");
        });
    }

    public override async Task HandleAsync(UpdateReviewRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateReviewCommand
        {
            ReviewId = req.ReviewId,
            Rating = req.Rating,
            Comment = req.Comment
        }, ct);

        await SendOkAsync(ApiResponse<object>.Ok(null!, "Review updated successfully."), ct);
    }
}
