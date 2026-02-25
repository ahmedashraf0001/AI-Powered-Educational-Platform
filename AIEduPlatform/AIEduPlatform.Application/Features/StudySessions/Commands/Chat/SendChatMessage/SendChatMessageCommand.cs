using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Chat.SendChatMessage
{
    public record SendChatMessageCommand
    {
        public Guid SessionId { get; init; }
        public string Message { get; init; } = string.Empty;
        public List<Guid>? LectureIds { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }

    public record ChatStreamContext
    {
        public IAsyncEnumerable<OllamaChatStreamChunk> Stream { get; init; } = null!;
        public List<string> Sources { get; init; } = [];
        public Guid SessionId { get; init; }
        public Guid CourseId { get; init; }
    }
}
