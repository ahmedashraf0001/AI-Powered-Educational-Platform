using System.Text;
using System.Text.Json;
using AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class SendChatMessageRequest
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<Guid>? LectureIds { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class SendChatMessageEndpoint : Endpoint<SendChatMessageRequest, object>
{
    private readonly IChatService _chatService;
    private readonly ILogger<SendChatMessageEndpoint> _logger;

    public SendChatMessageEndpoint(
        IChatService chatService,
        ILogger<SendChatMessageEndpoint> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/study-sessions/{SessionId}/chat");
        Group<StudySessionsGroup>();
        Summary(s =>
        {
            s.Summary = "Send a chat message (SSE streaming)";
            s.Description = "Sends a message to the AI tutor and streams the response via Server-Sent Events. Uses RAG to ground answers in course materials.";
            s.ExampleRequest = new SendChatMessageRequest
            {
                SessionId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
                Message = "Explain the difference between supervised and unsupervised learning."
            };
            s.Response(200, "SSE stream of AI response chunks");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(SendChatMessageRequest req, CancellationToken ct)
    {
        var context = await _chatService.PrepareStreamAsync(new SendChatMessageCommand
        {
            SessionId = req.SessionId,
            Message = req.Message,
            LectureIds = req.LectureIds,
            MaterialIds = req.MaterialIds
        }, ct);

        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache";
        HttpContext.Response.Headers.Connection = "keep-alive";

        var fullResponse = new StringBuilder();

        try
        {
            await foreach (var chunk in context.Stream)
            {
                var content = chunk.Message?.Content ?? string.Empty;
                fullResponse.Append(content);

                var eventData = JsonSerializer.Serialize(new { content, done = false });
                await HttpContext.Response.WriteAsync($"data: {eventData}\n\n", ct);
                await HttpContext.Response.Body.FlushAsync(ct);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error during chat streaming for session {SessionId}", req.SessionId);

            var errorData = JsonSerializer.Serialize(new
            {
                content = "",
                done = true,
                error = "An error occurred while generating the response."
            });
            await HttpContext.Response.WriteAsync($"data: {errorData}\n\n", ct);
            await HttpContext.Response.Body.FlushAsync(ct);
            return;
        }

        var finalEventData = JsonSerializer.Serialize(new
        {
            content = "",
            done = true,
            sources = context.Sources.Count > 0 ? context.Sources : null as List<string>
        });
        await HttpContext.Response.WriteAsync($"data: {finalEventData}\n\n", ct);
        await HttpContext.Response.Body.FlushAsync(ct);

        await _chatService.SaveMessagesAsync(
            context.SessionId, req.Message, fullResponse.ToString(), context.Sources, ct);

        _logger.LogInformation("Streamed chat response for session {SessionId}", req.SessionId);
    }
}
