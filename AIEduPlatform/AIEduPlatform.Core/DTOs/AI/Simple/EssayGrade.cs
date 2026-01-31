using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Simple;

/// <summary>
/// Criterion breakdown for essay grading
/// </summary>
public record GradingCriterion
{
    /// <summary>
    /// Name of the grading criterion (e.g., "Content Accuracy", "Completeness")
    /// </summary>
    [JsonPropertyName("criterionName")]
    public string CriterionName { get; init; } = string.Empty;

    /// <summary>
    /// Score awarded for this criterion
    /// </summary>
    [JsonPropertyName("score")]
    public float Score { get; init; }

    /// <summary>
    /// Maximum score for this criterion
    /// </summary>
    [JsonPropertyName("maxScore")]
    public float MaxScore { get; init; }

    /// <summary>
    /// Specific feedback for this criterion
    /// </summary>
    [JsonPropertyName("feedback")]
    public string Feedback { get; init; } = string.Empty;
}

/// <summary>
/// Simple essay grading DTO matching the exact JSON format returned by AI prompts
/// </summary>
public record EssayGrade
{
    /// <summary>
    /// Score awarded (out of maxPoints)
    /// </summary>
    [JsonPropertyName("score")]
    public float Score { get; init; }

    /// <summary>
    /// Maximum possible points
    /// </summary>
    [JsonPropertyName("maxPoints")]
    public int MaxPoints { get; init; }

    /// <summary>
    /// Percentage score
    /// </summary>
    [JsonPropertyName("percentage")]
    public float Percentage { get; init; }

    /// <summary>
    /// Comprehensive paragraph of feedback for the student
    /// </summary>
    [JsonPropertyName("feedback")]
    public string Feedback { get; init; } = string.Empty;

    /// <summary>
    /// Breakdown of scoring by criteria
    /// </summary>
    [JsonPropertyName("criteriaBreakdown")]
    public List<GradingCriterion> CriteriaBreakdown { get; init; } = new();

    /// <summary>
    /// Specific strengths demonstrated in the answer
    /// </summary>
    [JsonPropertyName("strengths")]
    public List<string> Strengths { get; init; } = new();

    /// <summary>
    /// Specific areas for improvement
    /// </summary>
    [JsonPropertyName("areasForImprovement")]
    public List<string> AreasForImprovement { get; init; } = new();

    /// <summary>
    /// Confidence level of the AI grading (0.0 to 1.0)
    /// </summary>
    [JsonPropertyName("confidence")]
    public float Confidence { get; init; }

    /// <summary>
    /// Whether teacher review is recommended
    /// </summary>
    [JsonPropertyName("requiresTeacherReview")]
    public bool RequiresTeacherReview { get; init; }

    /// <summary>
    /// Optional: reason why teacher review is needed
    /// </summary>
    [JsonPropertyName("reviewReason")]
    public string? ReviewReason { get; init; }
}
