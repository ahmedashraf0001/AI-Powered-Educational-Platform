using AIEduPlatform.Application.Features.Courses.Queries.Progress.GetCourseProgress;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Progress;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCourseProgressRequest
{
    public Guid CourseId { get; set; }
}

public class GetCourseProgressEndpoint : Endpoint<GetCourseProgressRequest, ApiResponse<CourseProgressDto>>
{
    private readonly IMediator _mediator;

    public GetCourseProgressEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}/progress");
        Roles("Student");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get course progress";
            s.Description = "Returns the authenticated student's progress for a specific course.";
            s.Response<ApiResponse<CourseProgressDto>>(200, "Course progress");
            s.Response(401, "Not authenticated");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseProgressRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseProgressQuery
        {
            CourseId = req.CourseId
        }, ct);

        await SendOkAsync(ApiResponse<CourseProgressDto>.Ok(result), ct);
    }
}
