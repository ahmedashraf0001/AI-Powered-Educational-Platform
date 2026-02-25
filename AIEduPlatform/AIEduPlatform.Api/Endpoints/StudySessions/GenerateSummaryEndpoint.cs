using AIEduPlatform.Application.Features.StudySessions.Commands.Summaries.GenerateSummary;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateSummaryRequest
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int SummaryLength { get; set; } = 500;
    public bool IncludeKeyPoints { get; set; } = true;
    public List<Guid>? LectureIds { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateSummaryEndpoint : Endpoint<GenerateSummaryRequest, ApiResponse<Summary>>
{
    private readonly IMediator _mediator;

    public GenerateSummaryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/summary");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate a topic summary with AI";
            s.Description = "Uses AI to generate a concise summary of a topic with optional key points, grounded in course materials.";
            s.Response<ApiResponse<Summary>>(200, "Generated summary");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GenerateSummaryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateSummaryCommand
        {
            SessionId = req.SessionId,
            Topic = req.Topic,
            SummaryLength = req.SummaryLength,
            IncludeKeyPoints = req.IncludeKeyPoints,
            LectureIds = req.LectureIds,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendOkAsync(ApiResponse<Summary>.Ok(result), ct);
    }
}
