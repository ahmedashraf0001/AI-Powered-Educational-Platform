using AIEduPlatform.Application.Features.Exams.Commands.Questions.GenerateAIQuestions;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Common;
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

public class GenerateAIQuestionsEndpoint : Endpoint<GenerateAIQuestionsRequest, ApiResponse<GenerateAIQuestionsResult>>
{
    private readonly IMediator _mediator;

    public GenerateAIQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/questions/generate-ai");
        Roles("Teacher");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate questions with AI";
            s.Description = "Uses AI to auto-generate exam questions from course materials. Optionally scope by lectures/materials, difficulty, and question types.";
            s.ExampleRequest = new GenerateAIQuestionsRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                NumberOfQuestions = 10,
                Difficulty = "medium",
                QuestionTypes = new List<QuestionType> { QuestionType.MultipleChoice, QuestionType.TrueFalse },
                FocusTopics = new List<string> { "Neural Networks", "Backpropagation" }
            };
            s.Response<ApiResponse<GenerateAIQuestionsResult>>(200, "Questions generated");
            s.Response(400, "AI generation failed");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
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
            await SendAsync(ApiResponse<GenerateAIQuestionsResult>.Fail(result.Error ?? "AI generation failed."), 400, ct);
            return;
        }

        await SendOkAsync(ApiResponse<GenerateAIQuestionsResult>.Ok(result), ct);
    }
}
