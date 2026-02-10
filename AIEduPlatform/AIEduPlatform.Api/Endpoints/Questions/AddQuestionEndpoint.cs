using AIEduPlatform.Application.Features.Exams.Commands.Questions.AddQuestion;
using AIEduPlatform.Core.Domain.Enums;
using FastEndpoints;
using MediatR;
using System.Text.Json;

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

public class AddQuestionEndpoint : Endpoint<AddQuestionRequest, Guid>
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
            s.Response<Guid>(201, "Question created — returns question ID");
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
            Options = req.Options != null ? JsonSerializer.Serialize(req.Options) : string.Empty,
            CorrectAnswer = req.CorrectAnswer,
            Points = req.Points
        }, ct);

        await SendCreatedAtAsync<GetExamQuestionsEndpoint>(
            new { examId = req.ExamId },
            result,
            cancellation: ct);
    }
}
