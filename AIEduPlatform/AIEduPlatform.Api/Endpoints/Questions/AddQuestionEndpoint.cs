using AIEduPlatform.Application.Features.Exams.Commands.Questions.AddQuestion;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class AddQuestionRequest
{
    public Guid ExamId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class AddQuestionResponse
{
    public Guid QuestionId { get; set; }
}

public class AddQuestionEndpoint : Endpoint<AddQuestionRequest, ApiResponse<AddQuestionResponse>>
{
    private readonly IMediator _mediator;

    public AddQuestionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/questions");
        Roles("Teacher");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Add a question to an exam";
            s.Description = "Creates a new question (MCQ, True/False, Short Answer, or Essay) for the specified exam.";
            s.ExampleRequest = new AddQuestionRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Type = QuestionType.MultipleChoice,
                Text = "Which activation function is most commonly used in hidden layers of deep neural networks?",
                Options = new List<string> { "Sigmoid", "ReLU", "Tanh", "Softmax" },
                CorrectAnswer = "ReLU",
                Points = 5
            };
            s.Response<ApiResponse<AddQuestionResponse>>(201, "Question created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(AddQuestionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddQuestionCommand
        {
            ExamId = req.ExamId,
            Type = req.Type,
            Text = req.Text,
            Options = req.Options ?? [],
            CorrectAnswer = req.CorrectAnswer,
            Points = req.Points
        }, ct);

        await SendCreatedAtAsync<GetExamQuestionsEndpoint>(
            new { examId = req.ExamId },
            ApiResponse<AddQuestionResponse>.Ok(new AddQuestionResponse { QuestionId = result }, "Question created successfully."),
            cancellation: ct);
    }
}
