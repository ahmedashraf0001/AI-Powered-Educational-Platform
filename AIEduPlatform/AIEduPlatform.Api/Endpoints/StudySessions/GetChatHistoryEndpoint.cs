using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Application.Features.StudySessions.Queries.Chat.GetChatHistory;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class GetChatHistoryRequest
{
    public Guid SessionId { get; set; }
}

public class GetChatHistoryEndpoint : Endpoint<GetChatHistoryRequest, ApiResponse<List<ChatMessageDto>>>
{
    private readonly IMediator _mediator;

    public GetChatHistoryEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/study-sessions/{SessionId}/chat");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Get chat history";
            s.Description = "Returns the full conversation history for a study session.";
            s.Response<ApiResponse<List<ChatMessageDto>>>(200, "Chat messages");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(GetChatHistoryRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetChatHistoryQuery
        {
            SessionId = req.SessionId
        }, ct);

        await SendOkAsync(ApiResponse<List<ChatMessageDto>>.Ok(result), ct);
    }
}
