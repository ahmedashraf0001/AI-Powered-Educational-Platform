using AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.StartSession;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class StartSessionRequest
{
    public Guid CourseId { get; set; }
}

public class StartSessionResponse
{
    public Guid SessionId { get; set; }
}

public class StartSessionEndpoint : Endpoint<StartSessionRequest, StartSessionResponse>
{
    private readonly IMediator _mediator;

    public StartSessionEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/study-sessions");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(StartSessionRequest req, CancellationToken ct)
    {
        var sessionId = await _mediator.Send(new StartSessionCommand
        {
            CourseId = req.CourseId
        }, ct);

        await SendAsync(new StartSessionResponse { SessionId = sessionId }, 201, ct);
    }
}
