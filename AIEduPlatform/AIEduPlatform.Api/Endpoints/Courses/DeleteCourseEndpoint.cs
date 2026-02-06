using AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class DeleteCourseRequest
{
    public Guid CourseId { get; set; }
}

public class DeleteCourseEndpoint : Endpoint<DeleteCourseRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/{courseId}");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(DeleteCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCourseCommand { CourseId = req.CourseId }, ct);
        await SendNoContentAsync(ct);
    }
}
