using AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class GetCourseEnrollmentsRequest
{
    public Guid CourseId { get; set; }
}

public class GetCourseEnrollmentsEndpoint : Endpoint<GetCourseEnrollmentsRequest, List<EnrollmentDto>>
{
    private readonly IMediator _mediator;

    public GetCourseEnrollmentsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{courseId}/enrollments");
        Group<EnrollmentsGroup>();
    }

    public override async Task HandleAsync(GetCourseEnrollmentsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseEnrollmentsQuery { CourseId = req.CourseId }, ct);
        await SendOkAsync(result, ct);
    }
}
