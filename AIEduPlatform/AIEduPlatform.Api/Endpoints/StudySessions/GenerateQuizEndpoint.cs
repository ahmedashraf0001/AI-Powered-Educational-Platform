using AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.GenerateQuiz;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateQuizRequest
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int NumberOfQuestions { get; set; } = 5;
    public string Difficulty { get; set; } = "medium";
    public List<string> QuestionTypes { get; set; } = new() { "mcq" };
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateQuizEndpoint : Endpoint<GenerateQuizRequest, ApiResponse<GeneratedQuizDto>>
{
    private readonly IMediator _mediator;

    public GenerateQuizEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/quizzes");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate a quiz with AI";
            s.Description = "Uses AI to generate a practice quiz on a topic. Supports MCQ, True/False, Short Answer, and Essay questions.";
            s.Response<ApiResponse<GeneratedQuizDto>>(200, "Generated quiz");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GenerateQuizRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateQuizCommand
        {
            SessionId = req.SessionId,
            Topic = req.Topic,
            NumberOfQuestions = req.NumberOfQuestions,
            Difficulty = req.Difficulty,
            QuestionTypes = req.QuestionTypes,
            LectureId = req.LectureId,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendAsync(ApiResponse<GeneratedQuizDto>.Ok(result), 201, ct);
    }
}
