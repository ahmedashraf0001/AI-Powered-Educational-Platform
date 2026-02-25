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
        Delete("/api/courses/{CourseId}");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a course";
            s.Description = "Permanently deletes a course and all its associated data. Only the course instructor can delete it.";
            s.Response(204, "Course deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(DeleteCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteCourseCommand { CourseId = req.CourseId }, ct);
        await SendNoContentAsync(ct);
    }
}
