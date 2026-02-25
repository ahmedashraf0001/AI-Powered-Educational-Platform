using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Ollama;

public class GroqOptions
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
    [JsonPropertyName("max_tokens")]
    public int? MaxTokens { get; init; }

    [JsonPropertyName("repeat_penalty")]
    public float? RepeatPenalty { get; init; }
}