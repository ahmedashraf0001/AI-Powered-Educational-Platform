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
        Get("/api/courses/{CourseId}");
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Get course details";
            s.Description = "Returns full course details including lectures and materials. User must be enrolled or be the instructor.";
            s.Response<CourseDetailDto>(200, "Course details");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled and not the instructor");
            s.Response(404, "Course not found");
        });
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
