using System.Text;
using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using FastEndpoints;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.Endpoints.StudySessions;

public class SendChatMessageRequest
{
    public Guid SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid? LectureId { get; set; }
    public List<Guid>? MaterialIds { get; set; }
}

public class SendChatMessageEndpoint : Endpoint<SendChatMessageRequest, object>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IOllamaServiceClient _ollamaClient;
    private readonly IRAGService _ragService;
    private readonly ILogger<SendChatMessageEndpoint> _logger;

    private const int MaxConversationHistory = 20;

    public SendChatMessageEndpoint(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IOllamaServiceClient ollamaClient,
        IRAGService ragService,
        ILogger<SendChatMessageEndpoint> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _ollamaClient = ollamaClient;
        _ragService = ragService;
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
            s.Response(200, "SSE stream of AI response chunks");
            s.Response(401, "Not authenticated");
            s.Response(403, "Not your session");
        });
    }

    public override async Task HandleAsync(SendChatMessageRequest req, CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
            throw new UnauthorizedException("You must be logged in.");

        var session = await _unitOfWork.StudySessions.GetByIdAsync(req.SessionId, ct);
        if (session is null)
            throw new NotFoundException(nameof(StudySession), req.SessionId);
        if (session.StudentId != userId.Value)
            throw new ForbiddenException("You can only chat in your own study sessions.");

        var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
        {
            Query = req.Message,
            CourseId = session.CourseId,
            LectureIds = req.LectureId.HasValue ? [req.LectureId.Value] : null,
            MaterialIds = req.MaterialIds
        }, ct);

        var recentMessages = await _unitOfWork.ChatMessages
            .GetRecentBySessionIdAsync(req.SessionId, MaxConversationHistory, ct);

        var conversationHistory = recentMessages
            .Select(m => new OllamaMessage
            {
                Role = m.Role == ChatRole.Student ? "user" : "assistant",
                Content = m.Content
            })
            .ToList();

        var sources = ragResponse.Chunks
            .Where(c => c.Metadata != null)
            .Select(c => c.Metadata!.SourceTitle)
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        var now = DateTime.UtcNow;
        await _unitOfWork.ChatMessages.AddAsync(new ChatMessage
        {
            SessionId = req.SessionId,
            Role = ChatRole.Student,
            Content = req.Message,
            Sources = null!,
            CreatedAt = now,
            UpdatedAt = now
        }, ct);

        HttpContext.Response.ContentType = "text/event-stream";
        HttpContext.Response.Headers.CacheControl = "no-cache";
        HttpContext.Response.Headers.Connection = "keep-alive";

        var fullResponse = new StringBuilder();

        try
        {
            await foreach (var chunk in _ollamaClient.GenerateStreamStudyChatResponseAsync(
                ragResponse.Chunks, req.Message, conversationHistory, ct))
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
            sources = sources.Count > 0 ? sources : null as List<string>
        });
        await HttpContext.Response.WriteAsync($"data: {finalEventData}\n\n", ct);
        await HttpContext.Response.Body.FlushAsync(ct);

        var sourcesJson = sources.Count > 0 ? JsonSerializer.Serialize(sources) : null;
        await _unitOfWork.ChatMessages.AddAsync(new ChatMessage
        {
            SessionId = req.SessionId,
            Role = ChatRole.System,
            Content = fullResponse.ToString(),
            Sources = sourcesJson!,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }, ct);

        await _unitOfWork.StudySessions.UpdateLastActivityAsync(req.SessionId, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation("Streamed chat response for session {SessionId}", req.SessionId);
    }
}
