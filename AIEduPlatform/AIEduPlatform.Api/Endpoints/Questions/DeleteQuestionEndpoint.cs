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
        Delete("/api/exams/questions/{QuestionId}");
        Roles("Teacher");
        Group<QuestionsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete a question";
            s.Description = "Permanently removes a question from an exam.";
            s.Response(204, "Question deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(DeleteQuestionRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteQuestionCommand { QuestionId = req.QuestionId }, ct);
        await SendNoContentAsync(ct);
    }
}
