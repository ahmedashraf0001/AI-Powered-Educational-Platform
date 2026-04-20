using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Rubric criterion for essay questions
/// </summary>
public record RubricCriterion
{
    /// <summary>
    /// Name of the criterion
    /// </summary>
    [JsonPropertyName("criterion")]
    public string Criterion { get; init; } = string.Empty;

    /// <summary>
    /// Maximum points for this criterion
    /// </summary>
    [JsonPropertyName("maxPoints")]
    public int MaxPoints { get; init; }

    /// <summary>
    /// Description of what this criterion assesses
    /// </summary>
    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;
}

/// <summary>
/// Simple exam question DTO matching the exact JSON format returned by AI prompts
/// Extended from quiz questions with additional exam-specific fields
/// </summary>
public record ExamQuestion
{
    /// <summary>
    /// The complete question text
    /// </summary>
    [JsonPropertyName("questionText")]
    public string QuestionText { get; init; } = string.Empty;

    /// <summary>
    /// Type: "mcq", "true_false", "short_answer", "essay"
    /// </summary>
    [JsonPropertyName("questionType")]
    public string QuestionType { get; init; } = "mcq";

    /// <summary>
    /// Options for MCQ (null for other types)
    /// </summary>
    [JsonPropertyName("options")]
    public List<string>? Options { get; init; }

    /// <summary>
    /// Correct answer (or expected answer for short_answer)
    /// </summary>
    [JsonPropertyName("correctAnswer")]
    public string CorrectAnswer { get; init; } = string.Empty;

    /// <summary>
    /// Detailed explanation of why this is correct and why others are wrong
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
    /// Grading criteria description
    /// </summary>
    [JsonPropertyName("gradingCriteria")]
    [JsonConverter(typeof(FlexibleStringJsonConverter))]
    public string? GradingCriteria { get; init; }

    /// <summary>
    /// Alternative answer field sometimes returned by LLMs for short/essay questions
    /// </summary>
    [JsonPropertyName("expectedAnswer")]
    public string? ExpectedAnswer { get; init; }

    /// <summary>
    /// Optional list of accepted answer variants
    /// </summary>
    [JsonPropertyName("acceptableVariations")]
    public List<string>? AcceptableVariations { get; init; }

    /// <summary>
    /// Model answer for essay questions
    /// </summary>
    [JsonPropertyName("modelAnswer")]
    public string? ModelAnswer { get; init; }

    /// <summary>
    /// Grading rubric for essay questions
    /// </summary>
    [JsonPropertyName("gradingRubric")]
    public List<RubricCriterion>? GradingRubric { get; init; }

    /// <summary>
    /// Title of the source material
    /// </summary>
    [JsonPropertyName("sourceTitle")]
    public string SourceTitle { get; init; } = string.Empty;

    /// <summary>
    /// Section name within the source
    /// </summary>
    [JsonPropertyName("sourceSection")]
    public string? SourceSection { get; init; }

    /// <summary>
    /// Location within the source material
    /// </summary>
    [JsonPropertyName("sourceLocation")]
    public string SourceLocation { get; init; } = string.Empty;

    /// <summary>
    /// What skill/knowledge this question assesses
    /// </summary>
    [JsonPropertyName("learningObjective")]
    public string? LearningObjective { get; init; }
}

internal sealed class FlexibleStringJsonConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.True => "true",
            JsonTokenType.False => "false",
            JsonTokenType.Number => ReadNumberAsString(ref reader),
            JsonTokenType.StartObject or JsonTokenType.StartArray => JsonDocument.ParseValue(ref reader).RootElement.GetRawText(),
            _ => throw new JsonException($"Unsupported token type {reader.TokenType} for flexible string conversion.")
        };
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value);
    }

    private static string ReadNumberAsString(ref Utf8JsonReader reader)
    {
        if (reader.TryGetInt64(out var intValue))
        {
            return intValue.ToString(CultureInfo.InvariantCulture);
        }

        if (reader.TryGetDecimal(out var decimalValue))
        {
            return decimalValue.ToString(CultureInfo.InvariantCulture);
        }

        return reader.GetDouble().ToString(CultureInfo.InvariantCulture);
    }
}
