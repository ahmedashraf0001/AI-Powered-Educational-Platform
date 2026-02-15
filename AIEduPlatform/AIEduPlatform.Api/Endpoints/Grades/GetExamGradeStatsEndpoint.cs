using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGradeStats;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetExamGradeStatsRequest
{
    public Guid ExamId { get; set; }
}

public class GetExamGradeStatsEndpoint : Endpoint<GetExamGradeStatsRequest, ApiResponse<ExamGradeStats>>
{
    private readonly IMediator _mediator;

    public GetExamGradeStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/grades/stats/exam/{ExamId}");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get exam grade statistics";
            s.Description = "Returns grade statistics for a specific exam including average, median, pass rate. Only the course instructor can view this.";
            s.Response<ApiResponse<ExamGradeStats>>(200, "Exam grade stats");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetExamGradeStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamGradeStatsQuery
        {
            ExamId = req.ExamId
        }, ct);
        await SendOkAsync(ApiResponse<ExamGradeStats>.Ok(result), ct);
    }
}
