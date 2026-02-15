using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCoursesByInstructorRequest
{
    public Guid InstructorId { get; set; }

    [QueryParam]
    public bool IncludeUnpublished { get; set; } = false;
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetCoursesByInstructorEndpoint : Endpoint<GetCoursesByInstructorRequest, ApiResponse<PagedResult<CourseListDto>>>
{
    private readonly IMediator _mediator;

    public GetCoursesByInstructorEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/instructor/{InstructorId}");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get courses by instructor";
            s.Description = "Returns all courses taught by a specific instructor with pagination. Optionally include unpublished courses.";
            s.Response<ApiResponse<PagedResult<CourseListDto>>>(200, "Instructor's courses");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetCoursesByInstructorRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCoursesByInstructorQuery
        {
            InstructorId = req.InstructorId,
            IncludeUnpublished = req.IncludeUnpublished,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<CourseListDto>>.Ok(result), ct);
    }
}
