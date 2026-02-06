using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCoursesByInstructorRequest
{
    public Guid InstructorId { get; set; }
    
    [QueryParam]
    public bool IncludeUnpublished { get; set; } = false;
}

public class GetCoursesByInstructorEndpoint : Endpoint<GetCoursesByInstructorRequest, List<CourseListDto>>
{
    private readonly IMediator _mediator;

    public GetCoursesByInstructorEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/instructor/{instructorId}");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(GetCoursesByInstructorRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCoursesByInstructorQuery
        {
            InstructorId = req.InstructorId,
            IncludeUnpublished = req.IncludeUnpublished
        }, ct);
        await SendOkAsync(result, ct);
    }
}
