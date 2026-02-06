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
        Put("/api/courses/{courseId}");
        Group<CoursesGroup>();
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
