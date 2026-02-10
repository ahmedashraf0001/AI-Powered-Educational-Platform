using AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Courses;

public class SearchCoursesRequest
{
    [QueryParam]
    public string Keyword { get; set; } = string.Empty;
}

public class SearchCoursesEndpoint : Endpoint<SearchCoursesRequest, List<CourseListDto>>
{
    private readonly IMediator _mediator;

    public SearchCoursesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/courses/search");
        AllowAnonymous();
        Group<CoursesGroup>();
        Summary(s =>
        {
            s.Summary = "Search courses";
            s.Description = "Searches published courses by keyword. No authentication required.";
            s.Response<List<CourseListDto>>(200, "Matching courses");
        });
    }

    public override async Task HandleAsync(SearchCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SearchCoursesQuery
        {
            Keyword = req.Keyword,
            OnlyPublished = true
        }, ct);
        await SendOkAsync(result, ct);
    }
}
