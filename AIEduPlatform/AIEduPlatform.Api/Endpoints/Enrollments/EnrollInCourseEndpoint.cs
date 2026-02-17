using AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class EnrollInCourseRequest
{
    public Guid CourseId { get; set; }
}

public class EnrollInCourseResponse
{
    public Guid EnrollmentId { get; set; }
}

public class EnrollInCourseEndpoint : EndpointWithoutRequest<ApiResponse<EnrollInCourseResponse>>
{
    private readonly IMediator _mediator;

    public EnrollInCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/courses/{CourseId}/enroll");
        Group<EnrollmentsGroup>();
        Summary(s =>
        {
            s.Summary = "Enroll in a course";
            s.Description = "Enrolls the authenticated user in the specified course.";
            s.Response<ApiResponse<EnrollInCourseResponse>>(200, "Enrolled successfully");
            s.Response(400, "Already enrolled");
            s.Response(401, "Not authenticated");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync( CancellationToken ct)
    {
        var courseId = Route<Guid>("CourseId");
        var enrollmentId = await _mediator.Send(new EnrollStudentCommand { CourseId = courseId }, ct);
        await SendOkAsync(ApiResponse<EnrollInCourseResponse>.Ok(
            new EnrollInCourseResponse { EnrollmentId = enrollmentId },
            "Enrolled successfully."), ct);
    }
}
