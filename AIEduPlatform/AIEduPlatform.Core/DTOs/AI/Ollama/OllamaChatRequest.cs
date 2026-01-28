using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama
{
    /// <summary>
    /// Low-level request body for Ollama /api/chat endpoint.
    /// Alternative to /generate when using chat-format models.
    /// </summary>
    public class OllamaChatRequest
    {
        /// <summary>
        /// The model name to use
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "qwen3:8b";

        /// <summary>
        /// The messages in the conversation
        /// </summary>
        [JsonPropertyName("messages")]
        public List<OllamaChatMessage> Messages { get; set; } = new List<OllamaChatMessage>();

        /// <summary>
        /// Whether to stream the response
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// Format for structured output (e.g., "json")
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// Generation options
        /// </summary>
        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }

        /// <summary>
        /// Keep model loaded in memory
        /// </summary>
        [JsonPropertyName("keep_alive")]
        public string? KeepAlive { get; set; }
    }

    /// <summary>
    /// A message in Ollama chat format
    /// </summary>
    public class OllamaChatMessage
    {
        /// <summary>
        /// Role: "system", "user", or "assistant"
        /// </summary>
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        /// <summary>
        /// The message content
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional images for multimodal models (base64 encoded)
        /// </summary>
        [JsonPropertyName("images")]
        public List<string>? Images { get; set; }

        /// <summary>
        /// Creates a system message
        /// </summary>
        public static OllamaChatMessage System(string content) =>
            new() { Role = "system", Content = content };

        /// <summary>
        /// Creates a user message
        /// </summary>
        public static OllamaChatMessage User(string content) =>
            new() { Role = "user", Content = content };

        /// <summary>
        /// Creates an assistant message
        /// </summary>
        public static OllamaChatMessage Assistant(string content) =>
            new() { Role = "assistant", Content = content };
    }
}
