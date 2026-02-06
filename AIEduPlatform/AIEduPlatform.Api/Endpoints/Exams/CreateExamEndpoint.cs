using AIEduPlatform.Application.Features.Exams.Commands.Exams.CreateExam;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Exams;

public class CreateExamRequest
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class CreateExamEndpoint : Endpoint<CreateExamRequest, Guid>
{
    private readonly IMediator _mediator;

    public CreateExamEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/exams");
        Group<ExamsGroup>();
    }

    public override async Task HandleAsync(CreateExamRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateExamCommand
        {
            CourseId = req.CourseId,
            Title = req.Title,
            StartTime = req.StartTime,
            EndTime = req.EndTime,
            DurationMinutes = req.DurationMinutes
        }, ct);
        
        await SendCreatedAtAsync<GetExamByIdEndpoint>(
            new { examId = result },
            result,
            cancellation: ct);
    }
}
