using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetStudentSessionsRequest
{
    [QueryParam]
    public Guid? CourseId { get; set; }
}

public class GetStudentSessionsEndpoint : Endpoint<GetStudentSessionsRequest, List<SessionSummaryDto>>
{
    private readonly IMediator _mediator;

    public GetStudentSessionsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get my study sessions";
            s.Description = "Returns all study sessions for the authenticated student. Optionally filter by course.";
            s.Response<List<SessionSummaryDto>>(200, "Study sessions");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetStudentSessionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentSessionsQuery
        {
            CourseId = req.CourseId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
