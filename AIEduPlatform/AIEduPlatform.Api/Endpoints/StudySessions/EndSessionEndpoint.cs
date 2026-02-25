using AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.EndSession;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class EndSessionRequest
{
    public Guid SessionId { get; set; }
}

public class EndSessionEndpoint : Endpoint<EndSessionRequest, ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public EndSessionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/end");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "End a study session";
            s.Description = "Explicitly ends an active study session. The session will no longer accept new messages or AI generation requests.";
            s.Response<ApiResponse<object>>(200, "Session ended");
            s.Response(400, "Session already ended");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
            s.Response(404, "Session not found");
        });
    }

    public override async Task HandleAsync(EndSessionRequest req, CancellationToken ct)
    {
        await _mediator.Send(new EndSessionCommand { SessionId = req.SessionId }, ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Study session ended successfully."), ct);
    }
}
