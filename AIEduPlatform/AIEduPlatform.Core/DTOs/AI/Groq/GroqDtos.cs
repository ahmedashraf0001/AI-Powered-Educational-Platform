using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Groq;

#region Request Types

/// <summary>
/// Groq chat completion request (OpenAI-compatible format).
/// </summary>
public record GroqChatRequest
{
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<GroqMessage> Messages { get; init; } = [];

    [JsonPropertyName("temperature")]
    public double? Temperature { get; init; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; init; }

    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; }

    [JsonPropertyName("stop")]
    public List<string>? Stop { get; init; }
}

public record GroqMessage
{
    [JsonPropertyName("role")]
    public string Role { get; init; } = "user";

    [JsonPropertyName("content")]
    public string Content { get; init; } = string.Empty;
}

#endregion

#region Response Types

/// <summary>
/// Groq chat completion response (OpenAI-compatible format).
/// </summary>
public record GroqChatResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; init; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; init; }

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<GroqChoice> Choices { get; init; } = [];

    [JsonPropertyName("usage")]
    public GroqUsage? Usage { get; init; }

    [JsonPropertyName("system_fingerprint")]
    public string? SystemFingerprint { get; init; }
}

public record GroqChoice
{
    [JsonPropertyName("index")]
    public int Index { get; init; }

    [JsonPropertyName("message")]
    public GroqMessage? Message { get; init; }

    [JsonPropertyName("delta")]
    public GroqDelta? Delta { get; init; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; init; }

    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; init; }
}

public record GroqDelta
{
    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("content")]
    public string? Content { get; init; }
}

public record GroqUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; init; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; init; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; init; }

    [JsonPropertyName("queue_time")]
    public double? QueueTime { get; init; }

    [JsonPropertyName("prompt_time")]
    public double? PromptTime { get; init; }

    [JsonPropertyName("completion_time")]
    public double? CompletionTime { get; init; }

    [JsonPropertyName("total_time")]
    public double? TotalTime { get; init; }
}

#endregion

#region Stream Types

/// <summary>
/// Groq streaming chunk (SSE data payload, OpenAI-compatible).
/// </summary>
public record GroqStreamChunk
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; init; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; init; }

    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    [JsonPropertyName("choices")]
    public List<GroqChoice> Choices { get; init; } = [];

    [JsonPropertyName("usage")]
    public GroqUsage? Usage { get; init; }

    [JsonPropertyName("x_groq")]
    public GroqExtension? XGroq { get; init; }
}

public record GroqExtension
{
    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("usage")]
    public GroqUsage? Usage { get; init; }
}

#endregion

#region Models API

/// <summary>
/// Response from GET /openai/v1/models
/// </summary>
public record GroqModelsResponse
{
    [JsonPropertyName("object")]
    public string Object { get; init; } = string.Empty;

    [JsonPropertyName("data")]
    public List<GroqModelInfo> Data { get; init; } = [];
}

public record GroqModelInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; init; } = string.Empty;

    [JsonPropertyName("created")]
    public long Created { get; init; }

    [JsonPropertyName("owned_by")]
    public string OwnedBy { get; init; } = string.Empty;

    [JsonPropertyName("active")]
    public bool Active { get; init; }
}

#endregion
