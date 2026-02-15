using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionMindMapsRequest
{
    public Guid SessionId { get; set; }
}

public class GetSessionMindMapsEndpoint : Endpoint<GetSessionMindMapsRequest, ApiResponse<List<MindMapDto>>>
{
    private readonly IMediator _mediator;

    public GetSessionMindMapsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/mindmaps");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get session mind maps";
            s.Description = "Returns all mind maps generated during this study session.";
            s.Response<ApiResponse<List<MindMapDto>>>(200, "Session mind maps");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GetSessionMindMapsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionMindMapsQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(ApiResponse<List<MindMapDto>>.Ok(result), ct);
    }
}
