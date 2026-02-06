using AIEduPlatform.Application.Features.Exams.Commands.Questions.DeleteQuestion;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Questions;

public class DeleteQuestionRequest
{
    public Guid QuestionId { get; set; }
}

public class DeleteQuestionEndpoint : Endpoint<DeleteQuestionRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteQuestionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/exams/questions/{questionId}");
        Group<QuestionsGroup>();
    }

    public override async Task HandleAsync(DeleteQuestionRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteQuestionCommand { QuestionId = req.QuestionId }, ct);
        await SendNoContentAsync(ct);
    }
}
