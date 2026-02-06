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
        Post("/api/exams/grades/{gradeId}/approve");
        Group<GradesGroup>();
    }

    public override async Task HandleAsync(ApproveGradeRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ApproveGradeCommand { GradeId = req.GradeId }, ct);
        await SendNoContentAsync(ct);
    }
}
