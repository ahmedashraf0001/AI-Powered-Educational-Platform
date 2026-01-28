using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama
{
    /// <summary>
    /// Response from Ollama /api/chat endpoint (non-streaming)
    /// </summary>
    public class OllamaChatResponse
    {
        /// <summary>
        /// The model that generated the response
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when response was created
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The assistant's response message
        /// </summary>
        [JsonPropertyName("message")]
        public OllamaChatMessage Message { get; set; } = new OllamaChatMessage();

        /// <summary>
        /// Whether generation is complete
        /// </summary>
        [JsonPropertyName("done")]
        public bool Done { get; set; }

        /// <summary>
        /// Reason for completion
        /// </summary>
        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        /// <summary>
        /// Total generation duration in nanoseconds
        /// </summary>
        [JsonPropertyName("total_duration")]
        public long TotalDuration { get; set; }

        /// <summary>
        /// Time to load model in nanoseconds
        /// </summary>
        [JsonPropertyName("load_duration")]
        public long LoadDuration { get; set; }

        /// <summary>
        /// Number of tokens in the prompt
        /// </summary>
        [JsonPropertyName("prompt_eval_count")]
        public int PromptEvalCount { get; set; }

        /// <summary>
        /// Time to evaluate prompt in nanoseconds
        /// </summary>
        [JsonPropertyName("prompt_eval_duration")]
        public long PromptEvalDuration { get; set; }

        /// <summary>
        /// Number of tokens in the response
        /// </summary>
        [JsonPropertyName("eval_count")]
        public int EvalCount { get; set; }

        /// <summary>
        /// Time to generate response in nanoseconds
        /// </summary>
        [JsonPropertyName("eval_duration")]
        public long EvalDuration { get; set; }

        /// <summary>
        /// Total duration in milliseconds (computed)
        /// </summary>
        [JsonIgnore]
        public long TotalDurationMs => TotalDuration / 1_000_000;
    }

    /// <summary>
    /// Streaming response chunk from Ollama /api/chat endpoint.
    /// Each chunk contains a partial message content.
    /// The final chunk (done=true) includes full statistics.
    /// </summary>
    public class OllamaChatStreamChunk
    {
        /// <summary>
        /// The model generating the response
        /// </summary>
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of this chunk
        /// </summary>
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// The partial message (contains role and content fragment)
        /// </summary>
        [JsonPropertyName("message")]
        public OllamaChatMessage Message { get; set; } = new OllamaChatMessage();

        /// <summary>
        /// Whether this is the final chunk
        /// </summary>
        [JsonPropertyName("done")]
        public bool Done { get; set; }

        /// <summary>
        /// Reason for completion (only in final chunk)
        /// </summary>
        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        // Statistics (only present in final chunk when done=true)

        /// <summary>
        /// Total generation duration in nanoseconds (final chunk only)
        /// </summary>
        [JsonPropertyName("total_duration")]
        public long? TotalDuration { get; set; }

        /// <summary>
        /// Time to load model in nanoseconds (final chunk only)
        /// </summary>
        [JsonPropertyName("load_duration")]
        public long? LoadDuration { get; set; }

        /// <summary>
        /// Number of tokens in the prompt (final chunk only)
        /// </summary>
        [JsonPropertyName("prompt_eval_count")]
        public int? PromptEvalCount { get; set; }

        /// <summary>
        /// Time to evaluate prompt in nanoseconds (final chunk only)
        /// </summary>
        [JsonPropertyName("prompt_eval_duration")]
        public long? PromptEvalDuration { get; set; }

        /// <summary>
        /// Number of tokens in the response (final chunk only)
        /// </summary>
        [JsonPropertyName("eval_count")]
        public int? EvalCount { get; set; }

        /// <summary>
        /// Time to generate response in nanoseconds (final chunk only)
        /// </summary>
        [JsonPropertyName("eval_duration")]
        public long? EvalDuration { get; set; }
    }
}
