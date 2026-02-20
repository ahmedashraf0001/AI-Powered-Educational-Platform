using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionQuizzesRequest
{
    public Guid SessionId { get; set; }
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    public int? PageSize { get; set; }
}

public class GetSessionQuizzesEndpoint : Endpoint<GetSessionQuizzesRequest, ApiResponse<PagedResult<GeneratedQuizDto>>>
{
    private readonly IMediator _mediator;

    public GetSessionQuizzesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/quizzes");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get session quizzes";
            s.Description = "Returns paginated quizzes generated during this study session, including scores if answered.";
            s.Response<ApiResponse<PagedResult<GeneratedQuizDto>>>(200, "Session quizzes");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GetSessionQuizzesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionQuizzesQuery
        {
            SessionId = req.SessionId,
            Page = req.Page ?? 1,
            PageSize = req.PageSize ?? 20
        }, ct);

        await SendOkAsync(ApiResponse<PagedResult<GeneratedQuizDto>>.Ok(result), ct);
    }
}
