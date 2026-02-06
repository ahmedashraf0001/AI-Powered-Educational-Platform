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
        Post("/api/courses/{courseId}/publish");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(PublishCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new PublishCourseCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(new PublishCourseResponse { Message = "Course published successfully." }, ct);
    }
}
