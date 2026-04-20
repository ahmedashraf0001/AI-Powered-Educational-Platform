using AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class DeleteCourseRequest
{
    public Guid CourseId { get; set; }

    [QueryParam]
    public CourseRemovalReason? Reason { get; set; }
}

public class DeleteCourseEndpoint : Endpoint<DeleteCourseRequest, ApiResponse<DeleteCourseResult>>
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
            s.Summary = "Delete or unpublish a course";
            s.Description = "If a course has sales history, it is unpublished instead of hard-deleted to preserve accounting records. Access revocation depends on removal reason.";
            s.Response<ApiResponse<DeleteCourseResult>>(200, "Delete policy applied");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(DeleteCourseRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new DeleteCourseCommand
        {
            CourseId = req.CourseId,
            Reason = req.Reason ?? CourseRemovalReason.InstructorRequest
        }, ct);

        await SendOkAsync(ApiResponse<DeleteCourseResult>.Ok(result, result.Message), ct);
    }
}
