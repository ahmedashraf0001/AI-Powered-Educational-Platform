using AIEduPlatform.Application.Features.Exams.Queries.Exams.GetPastExams;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class GetPastExamsRequest
{
    public Guid CourseId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetPastExamsEndpoint : Endpoint<GetPastExamsRequest, ApiResponse<PagedResult<ExamDto>>>
{
    private readonly IMediator _mediator;

    public GetPastExamsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/past/{CourseId}");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Get past exams";
            s.Description = "Returns exams that have already ended for a course.";
            s.Response<ApiResponse<PagedResult<ExamDto>>>(200, "Past exams");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetPastExamsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPastExamsQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<ExamDto>>.Ok(result), ct);
    }
}
