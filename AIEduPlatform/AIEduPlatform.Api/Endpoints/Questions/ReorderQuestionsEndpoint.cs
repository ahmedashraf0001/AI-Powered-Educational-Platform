using AIEduPlatform.Application.Features.Exams.Commands.Questions.ReorderQuestions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class ReorderQuestionsRequest
{
    public Guid ExamId { get; set; }
    public Dictionary<Guid, int> QuestionOrders { get; set; } = [];
}

public class ReorderQuestionsEndpoint : Endpoint<ReorderQuestionsRequest, object>
{
    private readonly IMediator _mediator;

    public ReorderQuestionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams/{ExamId}/questions/reorder");
        Roles("Teacher");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Reorder exam questions";
            s.Description = "Changes the display order of questions in an exam. Send a map of questionId to new order index.";
            s.Response(204, "Questions reordered");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(ReorderQuestionsRequest req, CancellationToken ct)
    {
        await _mediator.Send(new ReorderQuestionsCommand
        {
            ExamId = req.ExamId,
            QuestionOrders = req.QuestionOrders
        }, ct);

        await SendNoContentAsync(ct);
    }
}
