using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage
{
    public interface IChatService
    {
        Task<ChatStreamContext> PrepareStreamAsync(SendChatMessageCommand command, CancellationToken ct);
        Task SaveMessagesAsync(Guid sessionId, string userMessage, string aiResponse, List<string> sources, CancellationToken ct);
    }

    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<ChatService> _logger;

        private const int MaxConversationHistory = 20;

        public ChatService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<ChatService> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<ChatStreamContext> PrepareStreamAsync(SendChatMessageCommand command, CancellationToken ct)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(command.SessionId, ct);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), command.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only chat in your own study sessions.");

            var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
            {
                Query = command.Message,
                CourseId = session.CourseId,
                LectureIds = command.LectureId.HasValue ? [command.LectureId.Value] : null,
                MaterialIds = command.MaterialIds
            }, ct);

            var recentMessages = await _unitOfWork.ChatMessages
                .GetRecentBySessionIdAsync(command.SessionId, MaxConversationHistory, ct);

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
                SessionId = command.SessionId,
                Role = ChatRole.Student,
                Content = command.Message,
                Sources = null!,
                CreatedAt = now,
                UpdatedAt = now
            }, ct);

            var stream = _ollamaClient.GenerateStreamStudyChatResponseAsync(
                ragResponse.Chunks, command.Message, conversationHistory, ct);

            return new ChatStreamContext
            {
                Stream = stream,
                Sources = sources,
                SessionId = command.SessionId,
                CourseId = session.CourseId
            };
        }

        public async Task SaveMessagesAsync(Guid sessionId, string userMessage, string aiResponse, List<string> sources, CancellationToken ct)
        {
            var sourcesJson = sources.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(sources) : string.Empty;
            await _unitOfWork.ChatMessages.AddAsync(new ChatMessage
            {
                SessionId = sessionId,
                Role = ChatRole.Assistant,
                Content = aiResponse,
                Sources = sourcesJson!,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }, ct);

            await _unitOfWork.StudySessions.UpdateLastActivityAsync(sessionId, ct);
            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
