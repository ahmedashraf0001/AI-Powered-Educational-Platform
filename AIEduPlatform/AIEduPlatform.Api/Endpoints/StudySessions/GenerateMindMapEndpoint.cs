using AIEduPlatform.Application.Features.StudySessions.Commands.MindMaps.GenerateMindMap;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateMindMapRequest
{
    public Guid SessionId { get; set; }
    public string CentralTopic { get; set; } = string.Empty;
    public int MaxDepth { get; set; } = 3;
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateMindMapEndpoint : Endpoint<GenerateMindMapRequest, MindMapDto>
{
    private readonly IMediator _mediator;

    public GenerateMindMapEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/mindmaps");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GenerateMindMapRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateMindMapCommand
        {
            SessionId = req.SessionId,
            CentralTopic = req.CentralTopic,
            MaxDepth = req.MaxDepth,
            LectureId = req.LectureId,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendAsync(result, 201, ct);
    }
}
