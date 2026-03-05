using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Enrollments;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class UnenrollFromCourseRequest
{
    public Guid CourseId { get; set; }
}

public class UnenrollFromCourseEndpoint : Endpoint<UnenrollFromCourseRequest, ApiResponse<UnenrollmentResultDto>>
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
            s.Description = "Removes the authenticated user's enrollment from the specified course. For paid courses, enforces a 10-day refund policy with progress-based refund calculation.";
            s.Response<ApiResponse<UnenrollmentResultDto>>(200, "Unenrollment result");
            s.Response(400, "Unenrollment denied or validation failed");
            s.Response(401, "Not authenticated");
            s.Response(404, "Enrollment not found");
        });
    }

    public override async Task HandleAsync(UnenrollFromCourseRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new UnenrollStudentCommand { CourseId = req.CourseId }, ct);
        if (result.Success)
            await SendOkAsync(ApiResponse<UnenrollmentResultDto>.Ok(result, result.Message), ct);
        else
            await SendAsync(ApiResponse<UnenrollmentResultDto>.Fail(result.Message), 400, ct);
    }
}
