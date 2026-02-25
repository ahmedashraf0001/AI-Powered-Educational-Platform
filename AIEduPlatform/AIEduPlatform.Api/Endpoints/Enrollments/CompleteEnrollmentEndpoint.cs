using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.CompleteEnrollment;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class CompleteEnrollmentRequest
{
    public Guid CourseId { get; set; }
}

public class CompleteEnrollmentEndpoint : Endpoint<CompleteEnrollmentRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public CompleteEnrollmentEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/complete");
        Group<EnrollmentsGroup>();
        Summary(s =>
        {
            s.Summary = "Mark course as completed";
            s.Description = "Marks the authenticated student's enrollment in the specified course as completed.";
            s.Response<ApiResponse<object>>(200, "Course marked as completed");
            s.Response(400, "Already completed or not active");
            s.Response(401, "Not authenticated");
            s.Response(404, "Enrollment not found");
        });
    }

    public override async Task HandleAsync(CompleteEnrollmentRequest req, CancellationToken ct)
    {
        await _mediator.Send(new CompleteEnrollmentCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Course marked as completed."), ct);
    }
}
