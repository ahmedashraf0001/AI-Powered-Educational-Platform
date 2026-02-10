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
        Summary(s =>
        {
            s.Summary = "Start a study session";
            s.Description = "Creates a new AI-powered study session for a course. Student must be enrolled in the course.";
            s.Response<StartSessionResponse>(201, "Session started — returns session ID");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not enrolled in the course");
        });
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
