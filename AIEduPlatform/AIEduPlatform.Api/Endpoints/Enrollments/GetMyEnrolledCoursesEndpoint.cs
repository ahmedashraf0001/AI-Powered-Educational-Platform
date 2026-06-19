using AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Enrollments;

public class GetMyEnrolledCoursesRequest
{
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
    
    [QueryParam]
    public string? SearchQuery { get; set; }
    
    [QueryParam]
    public string? SortBy { get; set; }
    
    [QueryParam]
    public bool? ShowDropped { get; set; }
}

public class GetMyEnrolledCoursesEndpoint : Endpoint<GetMyEnrolledCoursesRequest, ApiResponse<PagedResult<EnrollmentDto>>>
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
            s.Response<ApiResponse<PagedResult<EnrollmentDto>>>(200, "Enrolled courses");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetMyEnrolledCoursesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetEnrolledCoursesQuery
        {
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<EnrollmentDto>>.Ok(result), ct);
    }
}
