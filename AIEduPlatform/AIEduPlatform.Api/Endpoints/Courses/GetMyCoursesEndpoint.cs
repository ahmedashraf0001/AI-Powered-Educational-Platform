using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetMyCoursesRequest
{
    [QueryParam]
    public bool IncludeUnpublished { get; set; } = true;
}

public class GetMyCoursesEndpoint : Endpoint<GetMyCoursesRequest, List<CourseListDto>>
{
    private readonly IMediator _mediator;

    public GetMyCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/my-courses");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(GetMyCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCoursesByInstructorQuery
        {
            IncludeUnpublished = req.IncludeUnpublished
        }, ct);
        await SendOkAsync(result, ct);
    }
}
