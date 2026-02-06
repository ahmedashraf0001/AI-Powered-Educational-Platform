using AIEduPlatform.Application.Features.Exams.Commands.Exams.UpdateExam;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class UpdateExamRequest
{
    public Guid ExamId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class UpdateExamEndpoint : Endpoint<UpdateExamRequest, object>
{
    private readonly IMediator _mediator;

    public UpdateExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Put("/api/exams/{examId}");
        Group<ExamsGroup>();
    }

    public override async Task HandleAsync(UpdateExamRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateExamCommand
        {
            ExamId = req.ExamId,
            Title = req.Title,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            DurationMinutes = req.DurationMinutes
        }, ct);

        await SendNoContentAsync(ct);
    }
}
