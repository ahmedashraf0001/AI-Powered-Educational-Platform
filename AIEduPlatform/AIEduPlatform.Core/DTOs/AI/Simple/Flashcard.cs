using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Simple flashcard DTO matching the exact JSON format returned by AI prompts
/// </summary>
public record Flashcard
{
    /// <summary>
    /// Front of the flashcard (question/term)
    /// </summary>
    [JsonPropertyName("front")]
    public string Front { get; init; } = string.Empty;

    /// <summary>
    /// Back of the flashcard (answer/definition)
    /// </summary>
    [JsonPropertyName("back")]
    public string Back { get; init; } = string.Empty;

    /// <summary>
    /// Difficulty level: "easy", "medium", "hard"
    /// </summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; init; } = "medium";

    /// <summary>
    /// Title of the source material
    /// </summary>
    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; init; } = string.Empty;

    /// <summary>
    /// Location within the source (page number, timestamp, etc.)
    /// </summary>
    [JsonPropertyName("sourceLocation")]
    public string SourceLocation { get; init; } = string.Empty;
}
