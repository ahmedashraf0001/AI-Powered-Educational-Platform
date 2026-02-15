using AIEduPlatform.Application.Features.StudySessions.Commands.MindMaps.GenerateMindMap;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
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

public class GenerateMindMapEndpoint : Endpoint<GenerateMindMapRequest, ApiResponse<MindMapDto>>
{
    private readonly IMediator _mediator;

    public GenerateMindMapEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/mindmaps");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate a mind map with AI";
            s.Description = "Uses AI to generate a structured mind map from a central topic, grounded in course materials.";
            s.Response<ApiResponse<MindMapDto>>(200, "Generated mind map");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
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

        await SendAsync(ApiResponse<MindMapDto>.Ok(result), 201, ct);
    }
}
