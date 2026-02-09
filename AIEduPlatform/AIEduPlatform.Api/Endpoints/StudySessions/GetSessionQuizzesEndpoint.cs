using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetSessionQuizzesRequest
{
    public Guid SessionId { get; set; }
}

public class GetSessionQuizzesEndpoint : Endpoint<GetSessionQuizzesRequest, List<GeneratedQuizDto>>
{
    private readonly IMediator _mediator;

    public GetSessionQuizzesEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/quizzes");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GetSessionQuizzesRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSessionQuizzesQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
