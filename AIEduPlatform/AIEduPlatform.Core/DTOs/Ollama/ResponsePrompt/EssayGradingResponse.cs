namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Essay Grading
    /// </summary>
    public class EssayGradingResponse : StructuredPromptResponse
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
        /// Score awarded (out of MaxPoints)
        /// </summary>
        public float Score { get; set; }

        /// <summary>
        /// Maximum possible points
        /// </summary>
        public int MaxPoints { get; set; }

        /// <summary>
        /// Percentage score
        /// </summary>
        public float Percentage => MaxPoints > 0 ? (Score / MaxPoints) * 100 : 0;

        /// <summary>
        /// Detailed feedback for the student
        /// </summary>
        public string Feedback { get; set; } = string.Empty;

        /// <summary>
        /// Breakdown of scoring criteria
        /// </summary>
        public List<GradingCriterion> CriteriaBreakdown { get; set; } = new List<GradingCriterion>();

        /// <summary>
        /// Strengths identified in the answer
        /// </summary>
        public List<string> Strengths { get; set; } = new List<string>();

        /// <summary>
        /// Areas for improvement
        /// </summary>
        public List<string> AreasForImprovement { get; set; } = new List<string>();

        /// <summary>
        /// Confidence level of the AI grading (0.0 to 1.0)
        /// </summary>
        public float Confidence { get; set; }

        /// <summary>
        /// Whether teacher review is recommended
        /// </summary>
        public bool RequiresTeacherReview { get; set; }
    }
}
