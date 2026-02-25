using AIEduPlatform.Application.Features.Reviews.Queries.GetCourseReviews;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Reviews;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public class GetCourseReviewsRequest
{
    public Guid CourseId { get; set; }

    [QueryParam]
    public int? Page { get; set; }

    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetCourseReviewsEndpoint : Endpoint<GetCourseReviewsRequest, ApiResponse<PagedResult<ReviewDto>>>
{
    private readonly IMediator _mediator;

    public GetCourseReviewsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}/reviews");
        AllowAnonymous();
        Group<ReviewsGroup>();
        Summary(s =>
        {
            s.Summary = "Get course reviews";
            s.Description = "Returns paginated reviews for a course. No authentication required.";
            s.Response<ApiResponse<PagedResult<ReviewDto>>>(200, "Paginated reviews");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseReviewsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseReviewsQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 10
        }, ct);

        await SendOkAsync(ApiResponse<PagedResult<ReviewDto>>.Ok(result), ct);
    }
}
