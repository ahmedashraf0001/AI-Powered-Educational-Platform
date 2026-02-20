using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionMindMapsRequest
{
    public Guid SessionId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetSessionMindMapsEndpoint : Endpoint<GetSessionMindMapsRequest, ApiResponse<PagedResult<MindMapDto>>>
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
            s.Description = "Returns paginated mind maps generated during this study session.";
            s.Response<ApiResponse<PagedResult<MindMapDto>>>(200, "Session mind maps");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GetSessionMindMapsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionMindMapsQuery
        {
            SessionId = req.SessionId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);

        await SendOkAsync(ApiResponse<PagedResult<MindMapDto>>.Ok(result), ct);
    }
}
