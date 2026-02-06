using AIEduPlatform.Application.Features.Exams.Commands.Questions.GenerateAIQuestions;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class GenerateAIQuestionsRequest
{
    public Guid ExamId { get; set; }
    public int NumberOfQuestions { get; set; }
    public string? Difficulty { get; set; }
    public List<QuestionType>? QuestionTypes { get; set; }
    public List<string>? FocusTopics { get; set; }
    public List<Guid>? LectureIds { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateAIQuestionsEndpoint : Endpoint<GenerateAIQuestionsRequest, GenerateAIQuestionsResult>
{
    private readonly IMediator _mediator;

    public GenerateAIQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{examId}/questions/generate-ai");
        Group<QuestionsGroup>();
    }

    public override async Task HandleAsync(GenerateAIQuestionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateAIQuestionsCommand
        {
            ExamId = req.ExamId,
            NumberOfQuestions = req.NumberOfQuestions,
            Difficulty = req.Difficulty,
            QuestionTypes = req.QuestionTypes,
            FocusTopics = req.FocusTopics,
            LectureIds = req.LectureIds,
            MaterialIds = req.MaterialIds
        }, ct);

        if (!result.Success)
        {
            await SendAsync(result, 400, ct);
            return;
        }

        await SendOkAsync(result, ct);
    }
}
