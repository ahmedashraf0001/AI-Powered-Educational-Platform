using AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentStats;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetStudentStatsRequest
{
    [QueryParam]
    public Guid? CourseId { get; set; }
}

public class GetStudentStatsEndpoint : Endpoint<GetStudentStatsRequest, ApiResponse<StudentSessionStats>>
{
    private readonly IMediator _mediator;

    public GetStudentStatsEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/stats");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get study session statistics";
            s.Description = "Returns aggregated study session statistics for the authenticated student. Optionally filter by course.";
            s.Response<ApiResponse<StudentSessionStats>>(200, "Session statistics");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetStudentStatsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentStatsQuery
        {
            CourseId = req.CourseId
        }, ct);

        await SendOkAsync(ApiResponse<StudentSessionStats>.Ok(result), ct);
    }
}
