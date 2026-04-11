using AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionFlashcards;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Sections;

public class GenerateSectionFlashcardsRequest
{
    public Guid SessionId { get; set; }
    public Guid SectionId { get; set; }
    public int NumberOfCards { get; set; } = 10;
}

public class GenerateSectionFlashcardsEndpoint : Endpoint<GenerateSectionFlashcardsRequest, ApiResponse<List<FlashcardDto>>>
{
    private readonly IMediator _mediator;

    public GenerateSectionFlashcardsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/sessions/{SessionId}/sections/{SectionId}/flashcards");
        Group<SectionsGroup>();
        Roles("Student", "Teacher");
        Summary(s =>
        {
            s.Summary = "Generate flashcards from a semantic section";
            s.Description = "Generates AI flashcards scoped to a specific semantic section.";
            s.ExampleRequest = new GenerateSectionFlashcardsRequest
            {
                SessionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                SectionId = Guid.Parse("b2c3d4e5-f6a7-8901-bcde-f12345678901"),
                NumberOfCards = 10
            };
            s.Response<ApiResponse<List<FlashcardDto>>>(200, "Section flashcards generated");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session or section not found");
        });
    }

    public override async Task HandleAsync(GenerateSectionFlashcardsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateSectionFlashcardsCommand
        {
            SessionId = req.SessionId,
            SectionId = req.SectionId,
            NumberOfCards = req.NumberOfCards
        }, ct);

        await SendOkAsync(ApiResponse<List<FlashcardDto>>.Ok(result, "Section flashcards generated"), ct);
    }
}
