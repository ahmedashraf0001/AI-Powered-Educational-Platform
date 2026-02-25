using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetStudentSessionsRequest
{
    [QueryParam]
    public Guid? CourseId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetStudentSessionsEndpoint : Endpoint<GetStudentSessionsRequest, ApiResponse<PagedResult<SessionSummaryDto>>>
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
            s.Response<ApiResponse<PagedResult<SessionSummaryDto>>>(200, "Study sessions");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(GetStudentSessionsRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetStudentSessionsQuery
        {
            CourseId = req.CourseId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 10
        }, ct);

        await SendOkAsync(ApiResponse<PagedResult<SessionSummaryDto>>.Ok(result), ct);
    }
}
