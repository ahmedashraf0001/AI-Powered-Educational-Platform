using AIEduPlatform.Application.Features.Exams.Commands.Questions.AddBulkQuestions;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class AddBulkQuestionsRequest
{
    public Guid ExamId { get; set; }
    public List<BulkQuestionItemRequest> Questions { get; set; } = [];
}

public class BulkQuestionItemRequest
{
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class AddBulkQuestionsResponse
{
    public List<Guid> QuestionIds { get; set; } = [];
}

public class AddBulkQuestionsEndpoint : Endpoint<AddBulkQuestionsRequest, ApiResponse<AddBulkQuestionsResponse>>
{
    private readonly IMediator _mediator;

    public AddBulkQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/questions/bulk");
        Roles("Teacher");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Add multiple questions to an exam";
            s.Description = "Creates multiple questions at once for the specified exam.";
            s.ExampleRequest = new AddBulkQuestionsRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Questions = new List<BulkQuestionItemRequest>
                {
                    new()
                    {
                        Type = QuestionType.MultipleChoice,
                        Text = "What is the time complexity of binary search?",
                        Options = new List<string> { "O(n)", "O(log n)", "O(n log n)", "O(1)" },
                        CorrectAnswer = "O(log n)",
                        Points = 5
                    },
                    new()
                    {
                        Type = QuestionType.TrueFalse,
                        Text = "A stack follows FIFO (First In, First Out) ordering.",
                        CorrectAnswer = "False",
                        Points = 3
                    }
                }
            };
            s.Response<ApiResponse<AddBulkQuestionsResponse>>(200, "Questions created");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(AddBulkQuestionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new AddBulkQuestionsCommand
        {
            ExamId = req.ExamId,
            Questions = req.Questions.Select(q => new BulkQuestionItem
            {
                Type = q.Type,
                Text = q.Text,
                Options = q.Options ?? [],
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points
            }).ToList()
        }, ct);

        await SendOkAsync(ApiResponse<AddBulkQuestionsResponse>.Ok(
            new AddBulkQuestionsResponse { QuestionIds = result },
            "Questions created successfully."), ct);
    }
}
