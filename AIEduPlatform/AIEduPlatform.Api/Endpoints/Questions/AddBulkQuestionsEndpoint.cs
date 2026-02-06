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
        Post("/api/exams/{examId}/questions/bulk");
        Group<QuestionsGroup>();
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
