using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Simple chat response DTO for study chat functionality
/// </summary>
public record ChatResponse
{
    /// <summary>
    /// The AI-generated response text
    /// </summary>
    [JsonPropertyName("response")]
    public string Response { get; init; } = string.Empty;

    /// <summary>
    /// Tokens used for prompt
    /// </summary>
    [JsonPropertyName("promptTokens")]
    public int PromptTokens { get; init; }

    /// <summary>
    /// Tokens used for response
    /// </summary>
    [JsonPropertyName("responseTokens")]
    public int ResponseTokens { get; init; }

    /// <summary>
    /// Total tokens used
    /// </summary>
    [JsonPropertyName("totalTokens")]
    public int TotalTokens { get; init; }

    /// <summary>
    /// Model used for generation
    /// </summary>
    [JsonPropertyName("model")]
    public string Model { get; init; } = string.Empty;
}