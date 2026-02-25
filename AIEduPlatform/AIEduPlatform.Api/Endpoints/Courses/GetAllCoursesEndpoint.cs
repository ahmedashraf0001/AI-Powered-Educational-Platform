using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetAllCoursesRequest
{
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetAllCoursesEndpoint : Endpoint<GetAllCoursesRequest, ApiResponse<PagedResult<CourseListDto>>>
{
    private readonly IMediator _mediator;

    public GetAllCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses");
        AllowAnonymous();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Browse all courses";
            s.Description = "Returns all published courses with pagination. No authentication required.";
            s.Response<ApiResponse<PagedResult<CourseListDto>>>(200, "List of published courses");
        });
    }

    public override async Task HandleAsync(GetAllCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCoursesQuery
        {
            OnlyPublished = true,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<CourseListDto>>.Ok(result), ct);
    }
}
