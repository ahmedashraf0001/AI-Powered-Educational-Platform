using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamsByCourse;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetExamsByCourseRequest
{
    public Guid CourseId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetExamsByCourseEndpoint : Endpoint<GetExamsByCourseRequest, ApiResponse<PagedResult<ExamDto>>>
{
    private readonly IMediator _mediator;

    public GetExamsByCourseEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/course/{CourseId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exams by course";
            s.Description = "Returns all exams for a specific course.";
            s.Response<ApiResponse<PagedResult<ExamDto>>>(200, "Course exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetExamsByCourseRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamsByCourseQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<ExamDto>>.Ok(result), ct);
    }
}
