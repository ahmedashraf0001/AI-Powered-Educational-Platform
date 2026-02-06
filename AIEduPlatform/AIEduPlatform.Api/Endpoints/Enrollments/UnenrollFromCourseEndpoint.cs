using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class UnenrollFromCourseRequest
{
    public Guid CourseId { get; set; }
}

public class UnenrollFromCourseResponse
{
    public string Message { get; set; } = string.Empty;
}

public class UnenrollFromCourseEndpoint : Endpoint<UnenrollFromCourseRequest, UnenrollFromCourseResponse>
{
    private readonly IMediator _mediator;

    public UnenrollFromCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/courses/{courseId}/unenroll");
        Group<EnrollmentsGroup>();
    }

    public override async Task HandleAsync(UnenrollFromCourseRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UnenrollStudentCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(new UnenrollFromCourseResponse { Message = "Unenrolled successfully." }, ct);
    }
}
