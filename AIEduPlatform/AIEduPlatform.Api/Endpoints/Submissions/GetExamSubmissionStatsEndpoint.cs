using AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissionStats;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class GetExamSubmissionStatsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamSubmissionStatsEndpoint : Endpoint<GetExamSubmissionStatsRequest, ApiResponse<SubmissionStats>>
{
    private readonly IMediator _mediator;

    public GetExamSubmissionStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/submissions/stats/{ExamId}");
        Roles("Teacher");
        Group<SubmissionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get exam submission statistics";
            s.Description = "Returns submission statistics for a specific exam including graded count, pending count, average score. Only the course instructor can view this.";
            s.Response<ApiResponse<SubmissionStats>>(200, "Submission stats");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetExamSubmissionStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamSubmissionStatsQuery
        {
            ExamId = req.ExamId
        }, ct);
        await SendOkAsync(ApiResponse<SubmissionStats>.Ok(result), ct);
    }
}
