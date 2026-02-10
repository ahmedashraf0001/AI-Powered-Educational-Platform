using AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class GetAllCoursesEndpoint : EndpointWithoutRequest<List<CourseListDto>>
{
    private readonly IMediator _mediator;

    public GetAllCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses");
        AllowAnonymous();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Browse all courses";
            s.Description = "Returns all published courses. No authentication required.";
            s.Response<List<CourseListDto>>(200, "List of published courses");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetAllCoursesQuery { OnlyPublished = true }, ct);
        await SendOkAsync(result, ct);
    }
}
