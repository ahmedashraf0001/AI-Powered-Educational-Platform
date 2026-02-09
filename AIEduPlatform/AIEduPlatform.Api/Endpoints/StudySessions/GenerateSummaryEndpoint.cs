using AIEduPlatform.Application.Features.StudySessions.Commands.Summaries.GenerateSummary;
using AIEduPlatform.Core.DTOs.AI.Simple;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateSummaryRequest
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int SummaryLength { get; set; } = 500;
    public bool IncludeKeyPoints { get; set; } = true;
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateSummaryEndpoint : Endpoint<GenerateSummaryRequest, Summary>
{
    private readonly IMediator _mediator;

    public GenerateSummaryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/summary");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GenerateSummaryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateSummaryCommand
        {
            SessionId = req.SessionId,
            Topic = req.Topic,
            SummaryLength = req.SummaryLength,
            IncludeKeyPoints = req.IncludeKeyPoints,
            LectureId = req.LectureId,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendOkAsync(result, ct);
    }
}
