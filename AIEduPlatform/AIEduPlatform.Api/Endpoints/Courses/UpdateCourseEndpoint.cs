using AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class UpdateCourseRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdateCourseEndpoint : Endpoint<UpdateCourseRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/courses/{CourseId}");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Update a course";
            s.Description = "Updates the title and description of a course. Only the course instructor can update it.";
            s.Response(204, "Course updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(UpdateCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateCourseCommand
        {
            CourseId = req.CourseId,
            Title = req.Title,
            Description = req.Description
        }, ct);

        await SendNoContentAsync(ct);
    }
}
