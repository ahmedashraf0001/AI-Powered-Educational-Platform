using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCourseById;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetCourseByIdRequest
{
    public Guid CourseId { get; set; }
    
    [QueryParam]
    public bool IncludeLectures { get; set; } = true;
    
    [QueryParam]
    public bool IncludeMaterials { get; set; } = true;
}

public class GetCourseByIdEndpoint : Endpoint<GetCourseByIdRequest, CourseDetailDto>
{
    private readonly IMediator _mediator;

    public GetCourseByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/{courseId}");
        Group<CoursesGroup>();
    }

    public override async Task HandleAsync(GetCourseByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetCourseByIdQuery
        {
            CourseId = req.CourseId,
            IncludeLectures = req.IncludeLectures,
            IncludeMaterials = req.IncludeMaterials
        }, ct);
        await SendOkAsync(result, ct);
    }
}
