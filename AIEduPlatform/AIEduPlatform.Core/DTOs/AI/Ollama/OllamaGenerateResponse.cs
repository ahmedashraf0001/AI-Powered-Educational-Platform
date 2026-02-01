using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama;

public record OllamaGenerateResponse
{
    /// <summary>
    /// The model that was used
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when generation was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The generated response text
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Whether the response is complete (for streaming)
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; init; }

    /// <summary>
    /// Reason for completion (e.g., "stop", "length")
    /// </summary>
    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; init; }

    /// <summary>
    /// Context tokens for conversation continuation
    /// </summary>
    [JsonPropertyName("context")]
    public IReadOnlyList<long>? Context { get; init; }

    /// <summary>
    /// Total duration in nanoseconds
    /// </summary>
    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; init; }

    /// <summary>
    /// Time spent loading the model in nanoseconds
    /// </summary>
    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; init; }

    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; init; }

    /// <summary>
    /// Time spent evaluating the prompt in nanoseconds
    /// </summary>
    [JsonPropertyName("prompt_eval_duration")]
    public long? PromptEvalDuration { get; init; }

    /// <summary>
    /// Number of tokens generated
    /// </summary>
    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; init; }

    /// <summary>
    /// Time spent generating in nanoseconds
    /// </summary>
    [JsonPropertyName("eval_duration")]
    public long? EvalDuration { get; init; }
}

public interface IOllamaStreamChunk
{
    public bool Done { get; init; }
}
public record OllamaGenerateStreamChunk : IOllamaStreamChunk
{
    /// <summary>
    /// The model being used
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp of this chunk
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The generated text chunk
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    [JsonPropertyName("done")]
    public bool Done { get; init; }

}



public record OllamaChatResponse
{
    /// <summary>
    /// The model that was used
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp when generation was created
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The generated message
    /// </summary>
    [JsonPropertyName("message")]
    public OllamaMessage Message { get; init; } = null!;

    /// <summary>
    /// Whether the response is complete (for streaming)
    /// </summary>
    [JsonPropertyName("done")]
    public bool Done { get; init; }

    /// <summary>
    /// Reason for completion (e.g., "stop", "length")
    /// </summary>
    [JsonPropertyName("done_reason")]
    public string? DoneReason { get; init; }

    /// <summary>
    /// Total duration in nanoseconds
    /// </summary>
    [JsonPropertyName("total_duration")]
    public long? TotalDuration { get; init; }

    /// <summary>
    /// Time spent loading the model in nanoseconds
    /// </summary>
    [JsonPropertyName("load_duration")]
    public long? LoadDuration { get; init; }

    /// <summary>
    /// Number of tokens in the prompt
    /// </summary>
    [JsonPropertyName("prompt_eval_count")]
    public int? PromptEvalCount { get; init; }

    /// <summary>
    /// Time spent evaluating the prompt in nanoseconds
    /// </summary>
    [JsonPropertyName("prompt_eval_duration")]
    public long? PromptEvalDuration { get; init; }

    /// <summary>
    /// Number of tokens generated
    /// </summary>
    [JsonPropertyName("eval_count")]
    public int? EvalCount { get; init; }

    /// <summary>
    /// Time spent generating in nanoseconds
    /// </summary>
    [JsonPropertyName("eval_duration")]
    public long? EvalDuration { get; init; }
}

public record OllamaMessage
{
    /// <summary>
    /// Role: "user", "assistant", or "system"
    /// </summary>
    [JsonPropertyName("role")]
    public string Role { get; init; } = "user";

    /// <summary>
    /// The message contentvar
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

public record OllamaChatStreamChunk: IOllamaStreamChunk
{
    /// <summary>
    /// The model being used
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// Timestamp of this chunk
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// The message chunk
    /// </summary>
    [JsonPropertyName("message")]
    public OllamaMessage Message { get; init; } = null!;
    [JsonPropertyName("done")]
    public bool Done { get; init; }
}
