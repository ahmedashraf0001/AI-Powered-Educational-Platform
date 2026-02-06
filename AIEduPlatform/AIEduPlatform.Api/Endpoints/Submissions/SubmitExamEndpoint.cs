using AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam;
using FastEndpoints;
using MediatR;
using System.Text.Json;

namespace AIEduPlatform.Api.Endpoints.Submissions;

public class SubmitExamRequest
{
    public Guid ExamId { get; set; }
    public Dictionary<Guid, string> Answers { get; set; } = [];
}

public class SubmitExamEndpoint : Endpoint<SubmitExamRequest, Guid>
{
    private readonly IMediator _mediator;

    public SubmitExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{examId}/submit");
        Group<SubmissionsGroup>();
    }

    public override async Task HandleAsync(SubmitExamRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitExamCommand
        {
            ExamId = req.ExamId,
            Answers = JsonSerializer.Serialize(req.Answers)
        }, ct);

        await SendCreatedAtAsync<GetSubmissionByIdEndpoint>(
            new { submissionId = result },
            result,
            cancellation: ct);
    }
}
