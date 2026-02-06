using AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class CreateCourseResponse
{
    public Guid CourseId { get; set; }
}

public class CreateCourseEndpoint : Endpoint<CreateCourseRequest, CreateCourseResponse>
{
    private readonly IMediator _mediator;

    public CreateCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(CreateCourseRequest req, CancellationToken ct)
    {
        var courseId = await _mediator.Send(new CreateCourseCommand
        {
            Title = req.Title,
            Description = req.Description
        }, ct);
        
        await SendCreatedAtAsync<GetCourseByIdEndpoint>(
            new { courseId },
            new CreateCourseResponse { CourseId = courseId },
            cancellation: ct);
    }
}
