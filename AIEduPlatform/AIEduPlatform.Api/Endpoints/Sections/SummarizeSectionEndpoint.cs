using AIEduPlatform.Application.Features.StudySessions.Commands.Sections.SummarizeSection;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Sections;

public class SummarizeSectionRequest
{
    public Guid SessionId { get; set; }
    public Guid SectionId { get; set; }
    public int SummaryLength { get; set; } = 500;
    public bool IncludeKeyPoints { get; set; } = true;
}

public class SummarizeSectionEndpoint : Endpoint<SummarizeSectionRequest, ApiResponse<Summary>>
{
    private readonly IMediator _mediator;

    public SummarizeSectionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/sessions/{SessionId}/sections/{SectionId}/summarize");
        Group<SectionsGroup>();
        Roles("Student", "Teacher");
        Summary(s =>
        {
            s.Summary = "Summarize a semantic section";
            s.Description = "Generates an AI summary of a specific semantic section using section-scoped RAG context.";
            s.ExampleRequest = new SummarizeSectionRequest
            {
                SessionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                SectionId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                SummaryLength = 500,
                IncludeKeyPoints = true
            };
            s.Response<ApiResponse<Summary>>(200, "Section summary");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session or section not found");
        });
    }

    public override async Task HandleAsync(SummarizeSectionRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new SummarizeSectionCommand
        {
            SessionId = req.SessionId,
            SectionId = req.SectionId,
            SummaryLength = req.SummaryLength,
            IncludeKeyPoints = req.IncludeKeyPoints
        }, ct);

        await SendOkAsync(ApiResponse<Summary>.Ok(result, "Section summary generated"), ct);
    }
}
