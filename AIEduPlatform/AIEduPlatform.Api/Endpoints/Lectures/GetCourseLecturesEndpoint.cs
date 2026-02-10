using AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetCourseLectures;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Lectures;

public class GetCourseLecturesRequest
{
    public Guid CourseId { get; set; }
    
    [QueryParam]
    public bool IncludeMaterials { get; set; } = true;
}

public class GetCourseLecturesEndpoint : Endpoint<GetCourseLecturesRequest, List<LectureDto>>
{
    private readonly IMediator _mediator;

    public GetCourseLecturesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{CourseId}/lectures");
        Group<LecturesGroup>();
        Summary(s =>
        {
            s.Summary = "Get course lectures";
            s.Description = "Returns all lectures for a course with optional materials. User must be enrolled or the instructor.";
            s.Response<List<LectureDto>>(200, "Course lectures");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
        });
    }

    public override async Task HandleAsync(GetCourseLecturesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseLecturesQuery
        {
            CourseId = req.CourseId,
            IncludeMaterials = req.IncludeMaterials
        }, ct);
        await SendOkAsync(result, ct);
    }
}
