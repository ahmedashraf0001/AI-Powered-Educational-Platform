using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeDistribution;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetGradeDistributionRequest
{
    public Guid ExamId { get; set; }
}

public class GetGradeDistributionEndpoint : Endpoint<GetGradeDistributionRequest, ApiResponse<Dictionary<string, int>>>
{
    private readonly IMediator _mediator;

    public GetGradeDistributionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/grades/distribution/{ExamId}");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get grade distribution for an exam";
            s.Description = "Returns grade distribution (A, B, C, D, F) for a specific exam. Only the course instructor can view this.";
            s.Response<ApiResponse<Dictionary<string, int>>>(200, "Grade distribution");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
            s.Response(404, "Exam not found");
        });
    }

    public override async Task HandleAsync(GetGradeDistributionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetGradeDistributionQuery
        {
            ExamId = req.ExamId
        }, ct);
        await SendOkAsync(ApiResponse<Dictionary<string, int>>.Ok(result), ct);
    }
}
