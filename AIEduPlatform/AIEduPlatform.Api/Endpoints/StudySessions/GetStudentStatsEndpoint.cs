using AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentStats;
using AIEduPlatform.Core.DTOs.Stats;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetStudentStatsRequest
{
    [QueryParam]
    public Guid? CourseId { get; set; }
}

public class GetStudentStatsEndpoint : Endpoint<GetStudentStatsRequest, StudentSessionStats>
{
    private readonly IMediator _mediator;

    public GetStudentStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/stats");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GetStudentStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentStatsQuery
        {
            CourseId = req.CourseId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
