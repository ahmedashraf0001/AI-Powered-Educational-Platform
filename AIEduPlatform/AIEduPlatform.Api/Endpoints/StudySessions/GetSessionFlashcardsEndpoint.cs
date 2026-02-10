using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Flashcards.GetSessionFlashcards;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionFlashcardsRequest
{
    public Guid SessionId { get; set; }
}

public class GetSessionFlashcardsEndpoint : Endpoint<GetSessionFlashcardsRequest, List<FlashcardDto>>
{
    private readonly IMediator _mediator;

    public GetSessionFlashcardsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/flashcards");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get session flashcards";
            s.Description = "Returns all flashcards generated during this study session.";
            s.Response<List<FlashcardDto>>(200, "Session flashcards");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GetSessionFlashcardsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionFlashcardsQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
