using AIEduPlatform.Application.Features.Courses.Queries.GetCourseEngagement;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCourseEngagementRequest
{
    public Guid CourseId { get; set; }
}

public class GetCourseEngagementEndpoint
    : Endpoint<GetCourseEngagementRequest, ApiResponse<CourseEngagementReport>>
{
    private readonly IMediator _mediator;

    public GetCourseEngagementEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}/engagement");
        Roles("Teacher");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get student engagement report for a course";
            s.Description = "Returns per-student engagement metrics including study sessions, " +
                            "AI interactions, exam performance, and an overall engagement score. " +
                            "Students are sorted by engagement (lowest first) so at-risk students " +
                            "appear at the top.";
            s.Response<ApiResponse<CourseEngagementReport>>(200, "Engagement report");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course teacher");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(GetCourseEngagementRequest req, CancellationToken ct)
    {
        var report = await _mediator.Send(new GetCourseEngagementQuery(req.CourseId), ct);
        await SendOkAsync(ApiResponse<CourseEngagementReport>.Ok(report), ct);
    }
}
