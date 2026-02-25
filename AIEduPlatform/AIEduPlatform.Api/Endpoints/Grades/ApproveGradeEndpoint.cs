using AIEduPlatform.Application.Features.Exams.Commands.Grades.ApproveGrade;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class ApproveGradeRequest
{
    public Guid GradeId { get; set; }
}

public class ApproveGradeEndpoint : Endpoint<ApproveGradeRequest, object>
{
    private readonly IMediator _mediator;

    public ApproveGradeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/grades/{GradeId}/approve");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Approve an AI grade";
            s.Description = "Approves an AI-generated grade, finalizing it. Only the course instructor can approve grades.";
            s.Response(204, "Grade approved");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(ApproveGradeRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ApproveGradeCommand { GradeId = req.GradeId }, ct);
        await SendNoContentAsync(ct);
    }
}
