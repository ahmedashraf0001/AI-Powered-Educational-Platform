using AIEduPlatform.Application.Features.Exams.Commands.Questions.AddBulkQuestions;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;
using System.Text.Json;

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

public class AddBulkQuestionsEndpoint : Endpoint<AddBulkQuestionsRequest, List<Guid>>
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
            s.Response<List<Guid>>(200, "Question IDs created");
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
                Options = q.Options != null ? JsonSerializer.Serialize(q.Options) : string.Empty,
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points
            }).ToList()
        }, ct);

        await SendAsync(result, cancellation: ct);
    }
}
