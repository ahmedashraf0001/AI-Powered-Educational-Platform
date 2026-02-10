using AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class PublishCourseRequest
{
    public Guid CourseId { get; set; }
}

public class PublishCourseResponse
{
    public string Message { get; set; } = string.Empty;
}

public class PublishCourseEndpoint : Endpoint<PublishCourseRequest, PublishCourseResponse>
{
    private readonly IMediator _mediator;

    public PublishCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/publish");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Publish a course";
            s.Description = "Makes a course visible to students. Only the course instructor can publish it.";
            s.Response<PublishCourseResponse>(200, "Course published");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(PublishCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new PublishCourseCommand { CourseId = req.CourseId, IsPublished = true }, ct);
        await SendOkAsync(new PublishCourseResponse { Message = "Course published successfully." }, ct);
    }
}
