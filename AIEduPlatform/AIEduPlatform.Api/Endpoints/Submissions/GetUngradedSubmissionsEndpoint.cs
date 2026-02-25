using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetUngradedSubmissions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetUngradedSubmissionsRequest
{
    [QueryParam]
    public Guid? ExamId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetUngradedSubmissionsEndpoint : Endpoint<GetUngradedSubmissionsRequest, ApiResponse<PagedResult<SubmissionDto>>>
{
    private readonly IMediator _mediator;

    public GetUngradedSubmissionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/submissions/ungraded");
        Roles("Teacher");
        Group<SubmissionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get ungraded submissions";
            s.Description = "Returns submissions that haven't been graded yet. Optionally filter by exam ID.";
            s.Response<ApiResponse<PagedResult<SubmissionDto>>>(200, "Ungraded submissions");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher role required");
        });
    }

    public override async Task HandleAsync(GetUngradedSubmissionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetUngradedSubmissionsQuery
        {
            ExamId = req.ExamId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<SubmissionDto>>.Ok(result), ct);
    }
}
