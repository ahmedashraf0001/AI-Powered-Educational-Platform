using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama
{
    /// <summary>
    /// Low-level request body for Ollama /api/generate endpoint.
    /// This contains the fully-built prompt string from PromptBuilder.
    /// </summary>
    public class OllamaGenerateRequest
    {
        /// <summary>
        /// The model name to use (e.g., "qwen3:8b", "llama3", "mistral")
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = "qwen3:8b";

        /// <summary>
        /// The fully-built prompt string (output from PromptBuilder)
        /// </summary>
        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Whether to stream the response
        /// </summary>
        [JsonPropertyName("stream")]
        public bool Stream { get; set; } = false;

        /// <summary>
        /// Optional system message (alternative to embedding in prompt)
        /// </summary>
        [JsonPropertyName("system")]
        public string? System { get; set; }

        /// <summary>
        /// Generation options
        /// </summary>
        [JsonPropertyName("options")]
        public OllamaOptions? Options { get; set; }

        /// <summary>
        /// Format for structured output (e.g., "json")
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// Context from previous request for conversation continuity
        /// </summary>
        [JsonPropertyName("context")]
        public int[]? Context { get; set; }

        /// <summary>
        /// Keep model loaded in memory (default: 5 minutes)
        /// </summary>
        [JsonPropertyName("keep_alive")]
        public string? KeepAlive { get; set; }
    }

    /// <summary>
    /// Ollama generation options
    /// </summary>
    public class OllamaOptions
    {
        /// <summary>
        /// Temperature (0.0 to 1.0) - higher = more creative
        /// </summary>
        [JsonPropertyName("temperature")]
        public float? Temperature { get; set; }

        /// <summary>
        /// Maximum tokens to generate
        /// </summary>
        [JsonPropertyName("num_predict")]
        public int? NumPredict { get; set; }

        /// <summary>
        /// Top-k sampling
        /// </summary>
        [JsonPropertyName("top_k")]
        public int? TopK { get; set; }

        /// <summary>
        /// Top-p (nucleus) sampling
        /// </summary>
        [JsonPropertyName("top_p")]
        public float? TopP { get; set; }

        /// <summary>
        /// Repetition penalty
        /// </summary>
        [JsonPropertyName("repeat_penalty")]
        public float? RepeatPenalty { get; set; }

        /// <summary>
        /// Random seed for reproducibility
        /// </summary>
        [JsonPropertyName("seed")]
        public int? Seed { get; set; }

        /// <summary>
        /// Stop sequences
        /// </summary>
        [JsonPropertyName("stop")]
        public List<string>? Stop { get; set; }
    }
}
