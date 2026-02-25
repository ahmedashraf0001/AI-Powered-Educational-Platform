using AIEduPlatform.Core.DTOs.AI.Ollama;

namespace AIEduPlatform.Core.DTOs.AI.Responses;

/// <summary>
/// Represents a split prompt with separate system and user messages
/// for use with Ollama's /api/chat endpoint.
/// </summary>
public record PromptResult
{
    /// <summary>
    /// The system message: role instructions, behavior guidelines, response format rules.
    /// </summary>
    public string SystemMessage { get; init; } = string.Empty;

    /// <summary>
    /// The user message: context chunks, parameters, and the actual request.
    /// </summary>
    public string UserMessage { get; init; } = string.Empty;

    /// <summary>
    /// Optional conversation history to insert as proper alternating user/assistant
    /// messages between the system message and the final user message.
    /// Used by study chat to give the LLM multi-turn conversational context
    /// via Ollama's native /api/chat message format instead of embedding
    /// history as text inside the user message.
    /// </summary>
    public List<OllamaMessage>? ConversationHistory { get; init; }
}
