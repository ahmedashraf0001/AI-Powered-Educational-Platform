using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetActiveExams;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetActiveExamsRequest
{
    public Guid CourseId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetActiveExamsEndpoint : Endpoint<GetActiveExamsRequest, ApiResponse<PagedResult<ExamDto>>>
{
    private readonly IMediator _mediator;

    public GetActiveExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/active/{CourseId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get active exams";
            s.Description = "Returns exams that are currently in progress for a course.";
            s.Response<ApiResponse<PagedResult<ExamDto>>>(200, "Active exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetActiveExamsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetActiveExamsQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<ExamDto>>.Ok(result), ct);
    }
}
