using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendedCourses;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetRecommendedCoursesRequest
{
    [QueryParam]
    public int? Top { get; set; }
}

public class GetRecommendedCoursesEndpoint : Endpoint<GetRecommendedCoursesRequest, ApiResponse<List<CourseListDto>>>
{
    private readonly IMediator _mediator;

    public GetRecommendedCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/recommended");
        Roles("Student", "Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get recommended courses";
            s.Description = "Returns personalized course recommendations for the authenticated user.";
            s.Response<ApiResponse<List<CourseListDto>>>(200, "Recommended courses");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetRecommendedCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetRecommendedCoursesQuery
        {
            Top = req.Top ?? 10
        }, ct);

        await SendOkAsync(ApiResponse<List<CourseListDto>>.Ok(result), ct);
    }
}
