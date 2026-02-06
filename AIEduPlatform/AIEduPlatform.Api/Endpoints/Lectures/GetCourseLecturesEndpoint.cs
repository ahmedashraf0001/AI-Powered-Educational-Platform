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
        Get("/api/courses/{courseId}/lectures");
        Group<LecturesGroup>();
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
