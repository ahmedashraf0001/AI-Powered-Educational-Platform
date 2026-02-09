using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Application.Features.StudySessions.Queries.Chat.GetChatHistory;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetChatHistoryRequest
{
    public Guid SessionId { get; set; }
}

public class GetChatHistoryEndpoint : Endpoint<GetChatHistoryRequest, List<ChatMessageDto>>
{
    private readonly IMediator _mediator;

    public GetChatHistoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/chat");
        Group<StudySessionsGroup>();
    }

    public override async Task HandleAsync(GetChatHistoryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatHistoryQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(result, ct);
    }
}
