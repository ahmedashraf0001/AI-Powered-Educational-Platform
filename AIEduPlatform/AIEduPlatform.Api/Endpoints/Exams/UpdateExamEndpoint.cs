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
            s.ExampleRequest = new UpdateExamRequest
            {
                ExamId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Title = "Final Exam — Machine Learning",
                StartTime = DateTime.UtcNow.AddDays(14),
                EndTime = DateTime.UtcNow.AddDays(14).AddHours(6),
                DurationMinutes = 120
            };
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
