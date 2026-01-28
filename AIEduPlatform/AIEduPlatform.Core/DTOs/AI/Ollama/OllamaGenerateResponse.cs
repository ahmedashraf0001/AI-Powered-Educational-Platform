using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama
{
    /// <summary>
    /// Response from Ollama /api/generate endpoint (non-streaming)
    /// </summary>
    public class OllamaGenerateResponse
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
        /// The generated text response
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        /// <summary>
        /// Whether generation is complete
        /// </summary>
        [JsonPropertyName("done")]
        public bool Done { get; set; }

        /// <summary>
        /// Reason for completion (e.g., "stop", "length")
        /// </summary>
        [JsonPropertyName("done_reason")]
        public string? DoneReason { get; set; }

        /// <summary>
        /// Context for conversation continuity
        /// </summary>
        [JsonPropertyName("context")]
        public int[]? Context { get; set; }

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

        /// <summary>
        /// Tokens per second (computed)
        /// </summary>
        [JsonIgnore]
        public double TokensPerSecond => EvalDuration > 0 
            ? EvalCount / (EvalDuration / 1_000_000_000.0) 
            : 0;
    }

    /// <summary>
    /// Streaming response chunk from Ollama /api/generate endpoint.
    /// Each chunk contains a partial response token.
    /// The final chunk (done=true) includes full statistics.
    /// </summary>
    public class OllamaGenerateStreamChunk
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
        /// The text token/chunk (partial response)
        /// </summary>
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

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

        /// <summary>
        /// Context for conversation continuity (only in final chunk)
        /// </summary>
        [JsonPropertyName("context")]
        public int[]? Context { get; set; }

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
