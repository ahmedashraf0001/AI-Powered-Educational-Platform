using AIEduPlatform.Application.Features.Exams.Commands.Exams.DeleteExam;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class DeleteExamRequest
{
    public Guid ExamId { get; set; }
}

public class DeleteExamEndpoint : Endpoint<DeleteExamRequest, object>
{
    private readonly IMediator _mediator;

    public DeleteExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/exams/{ExamId}");
        Roles("Teacher");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Delete an exam";
            s.Description = "Permanently deletes an exam, its questions, and all submissions. Only the course instructor can delete it.";
            s.Response(204, "Exam deleted");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
    }

    public override async Task HandleAsync(DeleteExamRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteExamCommand { ExamId = req.ExamId }, ct);
        await SendNoContentAsync(ct);
    }
}
