using AIEduPlatform.Application.Features.Exams.Commands.Grades.UpdateGrade;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class UpdateGradeRequest
{
    public Guid GradeId { get; set; }
    public float Score { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

public class UpdateGradeEndpoint : Endpoint<UpdateGradeRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateGradeEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/exams/grades/{gradeId}");
        Group<GradesGroup>();
    }

    public override async Task HandleAsync(UpdateGradeRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateGradeCommand
        {
            GradeId = req.GradeId,
            Score = req.Score,
            Feedback = req.Feedback
        }, ct);

        await SendNoContentAsync(ct);
    }
}
