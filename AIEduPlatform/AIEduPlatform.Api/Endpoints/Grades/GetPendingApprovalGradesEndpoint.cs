using AIEduPlatform.Application.Features.Exams.Queries.Grades.GetPendingApprovalGrades;
using AIEduPlatform.Core.DTOs.Exams;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GetPendingApprovalGradesRequest
{
    [QueryParam]
    public Guid? ExamId { get; set; }
}

public class GetPendingApprovalGradesEndpoint : Endpoint<GetPendingApprovalGradesRequest, List<GradeDto>>
{
    private readonly IMediator _mediator;

    public GetPendingApprovalGradesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/exams/grades/pending-approval");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Get grades pending approval";
            s.Description = "Returns AI-graded submissions awaiting teacher review and approval. Optionally filter by exam.";
            s.Response<List<GradeDto>>(200, "Pending approval grades");
            s.Response(401, "Not authenticated");
            s.Response(403, "Teacher role required");
        });
    }

    public override async Task HandleAsync(GetPendingApprovalGradesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetPendingApprovalGradesQuery { ExamId = req.ExamId }, ct);
        await SendOkAsync(result, ct);
    }
}
