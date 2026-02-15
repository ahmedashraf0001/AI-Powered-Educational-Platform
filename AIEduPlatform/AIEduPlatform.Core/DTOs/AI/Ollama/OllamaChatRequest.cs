using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama;

/// <summary>
/// Request DTO for Ollama /api/chat endpoint.
/// Uses a messages array with system/user/assistant roles.
/// </summary>
public record OllamaChatRequest
{
    /// <summary>
    /// The model name to use for generation (e.g., "llama3.2", "mistral")
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;

    /// <summary>
    /// The messages array (system, user, assistant roles)
    /// </summary>
    [JsonPropertyName("messages")]
    public List<OllamaMessage> Messages { get; init; } = new();

    /// <summary>
    /// Whether to stream the response (default: false)
    /// </summary>
    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = false;

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

    /// <summary>
    /// For thinking models (e.g. qwen3): whether the model should think before responding.
    /// Set to false to disable thinking tags in the output.
    /// </summary>
    [JsonPropertyName("think")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? Think { get; init; }
}
