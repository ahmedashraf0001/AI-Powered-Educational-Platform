using AIEduPlatform.Application.Features.Reviews.Commands.AddReview;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public class AddReviewRequest
{
    public Guid CourseId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class AddReviewResponse
{
    public Guid ReviewId { get; set; }
}

public class AddReviewEndpoint : Endpoint<AddReviewRequest, ApiResponse<AddReviewResponse>>
{
    private readonly IMediator _mediator;

    public AddReviewEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/reviews");
        Roles("Student", "Teacher");
        Group<ReviewsGroup>();
        Summary(s =>
        {
            s.Summary = "Add a review";
            s.Description = "Add a review and rating for a course. Must be enrolled in the course. One review per student per course.";
            s.ExampleRequest = new AddReviewRequest
            {
                CourseId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Rating = 5,
                Comment = "Excellent course! The AI study tools made learning much more interactive and effective."
            };
            s.Response<ApiResponse<AddReviewResponse>>(201, "Review created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled in the course");
            s.Response(409, "Already reviewed this course");
        });
    }

    public override async Task HandleAsync(AddReviewRequest req, CancellationToken ct)
    {
        var reviewId = await _mediator.Send(new AddReviewCommand
        {
            CourseId = req.CourseId,
            Rating = req.Rating,
            Comment = req.Comment
        }, ct);

        await SendAsync(
            ApiResponse<AddReviewResponse>.Ok(new AddReviewResponse { ReviewId = reviewId }, "Review added successfully."),
            201, ct);
    }
}
