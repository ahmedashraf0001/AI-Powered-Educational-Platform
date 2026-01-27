namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// A grading criterion with score
    /// </summary>
    public class GradingCriterion
    {
        /// <summary>
        /// Name of the criterion
        /// </summary>
        public string CriterionName { get; set; } = string.Empty;

        /// <summary>
        /// Score for this criterion
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Maximum score for this criterion
        /// </summary>
        public float MaxScore { get; set; }

        /// <summary>
        /// Feedback specific to this criterion
        /// </summary>
        public string Feedback { get; set; } = string.Empty;
    }
}
