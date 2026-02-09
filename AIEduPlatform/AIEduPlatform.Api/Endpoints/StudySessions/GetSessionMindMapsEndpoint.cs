using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionMindMapsRequest
{
    public Guid SessionId { get; set; }
}

public class GetSessionMindMapsEndpoint : Endpoint<GetSessionMindMapsRequest, List<MindMapDto>>
{
    private readonly IMediator _mediator;

    public GetSessionMindMapsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/mindmaps");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GetSessionMindMapsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionMindMapsQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
