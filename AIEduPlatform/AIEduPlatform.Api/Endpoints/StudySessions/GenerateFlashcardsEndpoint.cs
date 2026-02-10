using AIEduPlatform.Application.Features.StudySessions.Commands.Flashcards.GenerateFlashcards;
using AIEduPlatform.Core.DTOs.StudySessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GenerateFlashcardsRequest
{
    public Guid SessionId { get; set; }
    public string Topic { get; set; } = string.Empty;
    public int NumberOfCards { get; set; } = 10;
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class GenerateFlashcardsEndpoint : Endpoint<GenerateFlashcardsRequest, List<FlashcardDto>>
{
    private readonly IMediator _mediator;

    public GenerateFlashcardsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/flashcards");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Generate flashcards with AI";
            s.Description = "Uses AI to generate flashcards on a topic from course materials. Optionally scope by lecture or materials.";
            s.Response<List<FlashcardDto>>(200, "Generated flashcards");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GenerateFlashcardsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GenerateFlashcardsCommand
        {
            SessionId = req.SessionId,
            Topic = req.Topic,
            NumberOfCards = req.NumberOfCards,
            LectureId = req.LectureId,
            MaterialIds = req.MaterialIds
        }, ct);

        await SendAsync(result, 201, ct);
    }
}
