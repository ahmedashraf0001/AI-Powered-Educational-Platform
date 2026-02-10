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
        Put("/api/exams/{ExamId}");
        Roles("Teacher");
        Group<ExamsGroup>();
        Summary(s =>
        {
            s.Summary = "Update an exam";
            s.Description = "Updates exam details (title, time window, duration). Only the course instructor can update it.";
            s.Response(204, "Exam updated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not the course instructor");
        });
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
