using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Grading
{
    /// <summary>
    /// Criterion breakdown as returned by AI - matches the Required JSON Response Format in prompt
    /// </summary>
    public class GradingCriterion
    {
        /// <summary>
        /// Name of the grading criterion (e.g., "Content Accuracy", "Completeness")
        /// </summary>
        [JsonPropertyName("criterionName")]
        public string CriterionName { get; set; } = string.Empty;

        /// <summary>
        /// Score awarded for this criterion
        /// </summary>
        [JsonPropertyName("score")]
        public float Score { get; set; }

        /// <summary>
        /// Maximum score for this criterion
        /// </summary>
        [JsonPropertyName("maxScore")]
        public float MaxScore { get; set; }

        /// <summary>
        /// Specific feedback for this criterion
        /// </summary>
        [JsonPropertyName("feedback")]
        public string Feedback { get; set; } = string.Empty;
    }

    /// <summary>
    /// Essay grading response as returned by AI - matches the Required JSON Response Format in prompt
    /// </summary>
    public class EssayGradingData
    {
        /// <summary>
        /// Score awarded (out of maxPoints)
        /// </summary>
        [JsonPropertyName("score")]
        public float Score { get; set; }

        /// <summary>
        /// Maximum possible points
        /// </summary>
        [JsonPropertyName("maxPoints")]
        public int MaxPoints { get; set; }

        /// <summary>
        /// Percentage score
        /// </summary>
        [JsonPropertyName("percentage")]
        public float Percentage { get; set; }

        /// <summary>
        /// Comprehensive paragraph of feedback for the student
        /// </summary>
        [JsonPropertyName("feedback")]
        public string Feedback { get; set; } = string.Empty;

        /// <summary>
        /// Breakdown of scoring by criteria
        /// </summary>
        [JsonPropertyName("criteriaBreakdown")]
        public List<GradingCriterion> CriteriaBreakdown { get; set; } = new List<GradingCriterion>();

        /// <summary>
        /// Specific strengths demonstrated in the answer
        /// </summary>
        [JsonPropertyName("strengths")]
        public List<string> Strengths { get; set; } = new List<string>();

        /// <summary>
        /// Specific areas for improvement
        /// </summary>
        [JsonPropertyName("areasForImprovement")]
        public List<string> AreasForImprovement { get; set; } = new List<string>();

        /// <summary>
        /// Confidence level of the AI grading (0.0 to 1.0)
        /// </summary>
        [JsonPropertyName("confidence")]
        public float Confidence { get; set; }

        /// <summary>
        /// Whether teacher review is recommended
        /// </summary>
        [JsonPropertyName("requiresTeacherReview")]
        public bool RequiresTeacherReview { get; set; }

        /// <summary>
        /// Optional: reason why teacher review is needed
        /// </summary>
        [JsonPropertyName("reviewReason")]
        public string? ReviewReason { get; set; }
    }

    /// <summary>
    /// Full response wrapper for Essay Grading
    /// </summary>
    public class EssayGradingResponse : ResponseBase
    {
        /// <summary>
        /// The submission ID that was graded
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// The question ID that was graded
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// The parsed grading data from AI
        /// </summary>
        public EssayGradingData Data { get; set; } = new EssayGradingData();
    }
}
