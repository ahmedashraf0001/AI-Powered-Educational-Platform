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
        Get("/api/courses/{CourseId}/enrollments");
        Roles("Teacher");
        Group<EnrollmentsGroup>();
        Summary(s =>
        {
            s.Summary = "Get course enrollments";
            s.Description = "Returns all students enrolled in a course. Only the course instructor can view this.";
            s.Response<List<EnrollmentDto>>(200, "Course enrollments");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseEnrollmentsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseEnrollmentsQuery { CourseId = req.CourseId }, ct);
        await SendOkAsync(result, ct);
    }
}
