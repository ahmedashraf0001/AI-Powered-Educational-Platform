namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Represents a chat message in conversation history
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// Role: "user" or "assistant"
        /// </summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// The message content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the message
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
