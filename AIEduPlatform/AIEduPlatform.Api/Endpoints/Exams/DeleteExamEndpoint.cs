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
        Delete("/api/exams/{examId}");
        Group<ExamsGroup>();
    }

    public override async Task HandleAsync(DeleteExamRequest req, CancellationToken ct)
    {
        await _mediator.Send(new DeleteExamCommand { ExamId = req.ExamId }, ct);
        await SendNoContentAsync(ct);
    }
}
