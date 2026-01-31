using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Simple quiz question DTO matching the exact JSON format returned by AI prompts
/// </summary>
public record QuizQuestion
{
    /// <summary>
    /// The question text
    /// </summary>
    [JsonPropertyName("questionText")]
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Type: "mcq", "true_false", "short_answer", "essay"
    /// </summary>
    [JsonPropertyName("questionType")]
    public string QuestionType { get; init; } = "mcq";

    /// <summary>
    /// Options for MCQ (null for other types). For true_false: ["True", "False"]
    /// </summary>
    [JsonPropertyName("options")]
    public List<string>? Options { get; init; }

    /// <summary>
    /// Correct answer (or expected answer for short_answer)
    /// </summary>
    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; init; } = string.Empty;

    /// <summary>
    /// Explanation of why this is correct
    /// </summary>
    [JsonPropertyName("explanation")]
    public string Explanation { get; init; } = string.Empty;

    /// <summary>
    /// Difficulty: "easy", "medium", "hard"
    /// </summary>
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; init; } = "medium";

    /// <summary>
    /// Suggested points for this question
    /// </summary>
    [JsonPropertyName("suggestedPoints")]
    public int SuggestedPoints { get; init; } = 1;

    /// <summary>
    /// Title of the source material
    /// </summary>
    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; init; } = string.Empty;

    /// <summary>
    /// Location within the source material
    /// </summary>
    [JsonPropertyName("sourceLocation")]
    public string SourceLocation { get; init; } = string.Empty;
}
