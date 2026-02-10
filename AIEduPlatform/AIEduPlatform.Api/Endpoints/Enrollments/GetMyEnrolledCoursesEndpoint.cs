using AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class GetMyEnrolledCoursesEndpoint : EndpointWithoutRequest<List<EnrollmentDto>>
{
    private readonly IMediator _mediator;

    public GetMyEnrolledCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/enrolled");
        Group<EnrollmentsGroup>();
        Summary(s =>
        {
            s.Summary = "Get my enrolled courses";
            s.Description = "Returns all courses the authenticated user is currently enrolled in.";
            s.Response<List<EnrollmentDto>>(200, "Enrolled courses");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEnrolledCoursesQuery(), ct);
        await SendOkAsync(result, ct);
    }
}
