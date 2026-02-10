using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetSessionById;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionByIdRequest
{
    public Guid SessionId { get; set; }
}

public class GetSessionByIdEndpoint : Endpoint<GetSessionByIdRequest, SessionDetailDto>
{
    private readonly IMediator _mediator;

    public GetSessionByIdEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get session details";
            s.Description = "Returns full session details including chat messages, flashcards, quizzes, and mind maps.";
            s.Response<SessionDetailDto>(200, "Session details");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session not found");
        });
    }

    public override async Task HandleAsync(GetSessionByIdRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionByIdQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
