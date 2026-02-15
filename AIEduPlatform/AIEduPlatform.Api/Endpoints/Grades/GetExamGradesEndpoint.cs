using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetExamGradesRequest
{
    public Guid ExamId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetExamGradesEndpoint : Endpoint<GetExamGradesRequest, ApiResponse<PagedResult<GradeDto>>>
{
    private readonly IMediator _mediator;

    public GetExamGradesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/{ExamId}/grades");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get all grades for an exam";
            s.Description = "Returns all student grades for a specific exam. Only the course instructor can view this.";
            s.Response<ApiResponse<PagedResult<GradeDto>>>(200, "Exam grades");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(GetExamGradesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetExamGradesQuery
        {
            ExamId = req.ExamId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 10
        }, ct);
        await SendOkAsync(ApiResponse<PagedResult<GradeDto>>.Ok(result), ct);
    }
}
