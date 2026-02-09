using AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.SubmitQuizAnswers;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class SubmitQuizAnswersRequest
{
    public Guid SessionId { get; set; }
    public Guid QuizId { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new();
}

public class SubmitQuizAnswersEndpoint : Endpoint<SubmitQuizAnswersRequest, QuizResultDto>
{
    private readonly IMediator _mediator;

    public SubmitQuizAnswersEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/quizzes/{QuizId}/submit");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(SubmitQuizAnswersRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitQuizAnswersCommand
        {
            SessionId = req.SessionId,
            QuizId = req.QuizId,
            Answers = req.Answers
        }, ct);

        await SendOkAsync(result, ct);
    }
}
