using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama;

/// <summary>
/// Request DTO for Ollama /api/generate endpoint
/// </summary>
public record OllamaGenerateRequest
{
    /// <summary>
    /// The model name to use for generation (e.g., "llama3.2", "mistral")
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// The prompt text to send to the model
    /// </summary>
    [JsonPropertyName("prompt")]
    public string Prompt { get; init; } = string.Empty;

    /// <summary>
    /// System message to set context/behavior (optional)
    /// </summary>
    [JsonPropertyName("system")]
    public string? System { get; init; }

    /// <summary>
    /// Template to use for the prompt (optional)
    /// </summary>
    [JsonPropertyName("template")]
    public string? Template { get; init; }

    /// <summary>
    /// Context from previous conversation (for continuation)
    /// </summary>
    [JsonPropertyName("context")]
    public List<long>? Context { get; init; }

    /// <summary>
    /// Whether to stream the response (default: true in Ollama)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = false;

    /// <summary>
    /// Raw mode - no formatting applied (default: false)
    /// </summary>
    [JsonPropertyName("raw")]
    public bool Raw { get; init; } = false;

    /// <summary>
    /// Format for the response (e.g., "json")
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format { get; init; }

    /// <summary>
    /// Keep-alive duration for the model in memory
    /// </summary>
    [JsonPropertyName("keep_alive")]
    public string? KeepAlive { get; init; }

    /// <summary>
    /// Generation options (temperature, top_p, etc.)
    /// </summary>
    [JsonPropertyName("options")]
    public OllamaOptions? Options { get; init; }
}

/// <summary>
/// Generation options for Ollama models
/// </summary>
public record OllamaOptions
{
    /// <summary>
    /// Temperature for sampling (0.0 to 2.0). Higher = more random
    /// </summary>
    [JsonPropertyName("temperature")]
    public float? Temperature { get; init; }

    /// <summary>
    /// Top-p sampling (nucleus sampling). Range: 0.0 to 1.0
    /// </summary>
    [JsonPropertyName("top_p")]
    public float? TopP { get; init; }

    /// <summary>
    /// Top-k sampling. Higher = more diverse
    /// </summary>
    [JsonPropertyName("top_k")]
    public int? TopK { get; init; }

    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    [JsonPropertyName("num_predict")]
    public int? NumPredict { get; init; }

    /// <summary>
    /// Stop sequences to end generation
    /// </summary>
    [JsonPropertyName("stop")]
    public List<string>? Stop { get; init; }

    /// <summary>
    /// Penalize repetition. Range: 0.0 to 2.0
    /// </summary>
    [JsonPropertyName("repeat_penalty")]
    public float? RepeatPenalty { get; init; }

    /// <summary>
    /// Random seed for reproducibility
    /// </summary>
    [JsonPropertyName("seed")]
    public int? Seed { get; init; }

    /// <summary>
    /// Context window size
    /// </summary>
    [JsonPropertyName("num_ctx")]
    public int? NumCtx { get; init; }
}
