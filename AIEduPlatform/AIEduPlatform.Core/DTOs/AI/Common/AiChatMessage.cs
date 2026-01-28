using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Common
{
    /// <summary>
    /// Chat message for conversation history - matches prompt builder format
    /// </summary>
    public class AiChatMessage
    {
        /// <summary>
        /// Role: "user" or "assistant"
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        /// <summary>
        /// The message content
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of the message (internal tracking)
        /// </summary>
        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a user message
        /// </summary>
        public static AiChatMessage User(string content) =>
            new() { Role = "user", Content = content };

        /// <summary>
        /// Creates an assistant message
        /// </summary>
        public static AiChatMessage Assistant(string content) =>
            new() { Role = "assistant", Content = content };
    }
}
