using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetMyCoursesRequest
{
    [QueryParam]
    public bool IncludeUnpublished { get; set; } = true;
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetMyCoursesEndpoint : Endpoint<GetMyCoursesRequest, ApiResponse<PagedResult<CourseListDto>>>
{
    private readonly IMediator _mediator;

    public GetMyCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/my-courses");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get my taught courses";
            s.Description = "Returns all courses created by the authenticated teacher, including unpublished drafts.";
            s.Response<ApiResponse<PagedResult<CourseListDto>>>(200, "Teacher's courses");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher role required");
        });
    }

    public override async Task HandleAsync(GetMyCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCoursesByInstructorQuery
        {
            IncludeUnpublished = req.IncludeUnpublished,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<CourseListDto>>.Ok(result), ct);
    }
}
