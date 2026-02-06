using AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmissionWithAI;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Grades;

public class GradeSubmissionWithAIRequest
{
    public Guid SubmissionId { get; set; }
}

public class GradeSubmissionWithAIEndpoint : Endpoint<GradeSubmissionWithAIRequest, GradeSubmissionWithAIResult>
{
    private readonly IMediator _mediator;

    public GradeSubmissionWithAIEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/submissions/{submissionId}/grade-ai");
        Group<GradesGroup>();
    }

    public override async Task HandleAsync(GradeSubmissionWithAIRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GradeSubmissionWithAICommand
        {
            SubmissionId = req.SubmissionId
        }, ct);

        if (!result.Success)
        {
            await SendAsync(result, 400, ct);
            return;
        }

        await SendOkAsync(result, ct);
    }
}
