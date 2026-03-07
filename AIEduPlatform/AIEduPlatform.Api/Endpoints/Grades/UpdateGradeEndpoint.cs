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
        Put("/api/exams/grades/{GradeId}");
        Roles("Teacher");
        Group<GradesGroup>();
        Summary(s =>
        {
            s.Summary = "Update a grade";
            s.Description = "Updates the score and feedback of an existing grade. Only the course instructor can modify grades.";
            s.ExampleRequest = new UpdateGradeRequest
            {
                GradeId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Score = 90.0f,
                Feedback = "Revised grade after re-evaluation. Excellent work on the practical section."
            };
            s.Response(204, "Grade updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
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
