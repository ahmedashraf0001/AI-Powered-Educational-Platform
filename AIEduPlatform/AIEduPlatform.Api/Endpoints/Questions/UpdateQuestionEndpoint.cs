using AIEduPlatform.Application.Features.Exams.Commands.Questions.UpdateQuestion;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;
using System.Text.Json;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class UpdateQuestionRequest
{
    public Guid QuestionId { get; set; }
    public QuestionType Type { get; set; }
    public string Text { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public string CorrectAnswer { get; set; } = string.Empty;
    public int Points { get; set; }
}

public class UpdateQuestionEndpoint : Endpoint<UpdateQuestionRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateQuestionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/exams/questions/{questionId}");
        Group<QuestionsGroup>();
    }

    public override async Task HandleAsync(UpdateQuestionRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateQuestionCommand
        {
            QuestionId = req.QuestionId,
            Type = req.Type,
            Text = req.Text,
            Options = req.Options != null ? JsonSerializer.Serialize(req.Options) : string.Empty,
            CorrectAnswer = req.CorrectAnswer,
            Points = req.Points
        }, ct);

        await SendNoContentAsync(ct);
    }
}
