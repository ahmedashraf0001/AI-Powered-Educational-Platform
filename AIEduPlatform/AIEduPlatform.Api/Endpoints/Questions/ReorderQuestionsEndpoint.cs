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
            s.ExampleRequest = new ReorderQuestionsRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                QuestionOrders = new Dictionary<Guid, int>
                {
                    { Guid.Parse("11111111-1111-1111-1111-111111111111"), 0 },
                    { Guid.Parse("22222222-2222-2222-2222-222222222222"), 1 },
                    { Guid.Parse("33333333-3333-3333-3333-333333333333"), 2 }
                }
            };
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
