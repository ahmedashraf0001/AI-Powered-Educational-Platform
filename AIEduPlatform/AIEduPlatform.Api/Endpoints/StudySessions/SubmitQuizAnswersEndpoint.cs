using AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.SubmitQuizAnswers;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class SubmitQuizAnswersRequest
{
    public Guid SessionId { get; set; }
    public Guid QuizId { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new();
}

public class SubmitQuizAnswersEndpoint : Endpoint<SubmitQuizAnswersRequest, ApiResponse<QuizResultDto>>
{
    private readonly IMediator _mediator;

    public SubmitQuizAnswersEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/quizzes/{QuizId}/submit");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Submit quiz answers";
            s.Description = "Submits answers for a generated quiz. MCQ/True-False are auto-graded; Short Answer/Essay are AI-graded.";
            s.ExampleRequest = new SubmitQuizAnswersRequest
            {
                SessionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                QuizId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                Answers = new Dictionary<int, string>
                {
                    { 0, "ReLU" },
                    { 1, "True" },
                    { 2, "Gradient descent is an optimization algorithm used to minimize the loss function." }
                }
            };
            s.Response<ApiResponse<QuizResultDto>>(200, "Quiz result with scores and AI feedback");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(SubmitQuizAnswersRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SubmitQuizAnswersCommand
        {
            SessionId = req.SessionId,
            QuizId = req.QuizId,
            Answers = req.Answers
        }, ct);

        await SendOkAsync(ApiResponse<QuizResultDto>.Ok(result), ct);
    }
}
