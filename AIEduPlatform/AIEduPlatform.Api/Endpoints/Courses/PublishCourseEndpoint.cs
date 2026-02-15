using AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class PublishCourseEndpoint : EndpointWithoutRequest<ApiResponse<object>>
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
            s.Response<ApiResponse<object>>(200, "Course published");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var courseId = Route<Guid>("CourseId");
        await _mediator.Send(new PublishCourseCommand { CourseId = courseId, IsPublished = true }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Course published successfully."), ct);
    }
}
