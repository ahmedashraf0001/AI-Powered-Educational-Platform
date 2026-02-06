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
        Post("/api/exams/{examId}/questions/reorder");
        Group<QuestionsGroup>();
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
