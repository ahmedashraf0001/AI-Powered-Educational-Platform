using AIEduPlatform.Application.Features.Reviews.Queries.GetCourseRatingSummary;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Reviews;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Reviews;

public class GetCourseRatingSummaryRequest
{
    public Guid CourseId { get; set; }
}

public class GetCourseRatingSummaryEndpoint : Endpoint<GetCourseRatingSummaryRequest, ApiResponse<CourseRatingSummaryDto>>
{
    private readonly IMediator _mediator;

    public GetCourseRatingSummaryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}/rating");
        AllowAnonymous();
        Group<ReviewsGroup>();
        Summary(s =>
        {
            s.Summary = "Get course rating summary";
            s.Description = "Returns the average rating, total reviews, and rating distribution for a course. No authentication required.";
            s.Response<ApiResponse<CourseRatingSummaryDto>>(200, "Rating summary");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseRatingSummaryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseRatingSummaryQuery
        {
            CourseId = req.CourseId
        }, ct);

        await SendOkAsync(ApiResponse<CourseRatingSummaryDto>.Ok(result), ct);
    }
}
