using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class UnenrollFromCourseRequest
{
    public Guid CourseId { get; set; }
}

public class UnenrollFromCourseEndpoint : Endpoint<UnenrollFromCourseRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public UnenrollFromCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/{CourseId}/unenroll");
        Group<EnrollmentsGroup>();
        Summary(s =>
        {
            s.Summary = "Unenroll from a course";
            s.Description = "Removes the authenticated user's enrollment from the specified course.";
            s.Response<ApiResponse<object>>(200, "Unenrolled successfully");
            s.Response(401, "Not authenticated");
            s.Response(404, "Enrollment not found");
        });
    }

    public override async Task HandleAsync(UnenrollFromCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UnenrollStudentCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Unenrolled successfully."), ct);
    }
}
