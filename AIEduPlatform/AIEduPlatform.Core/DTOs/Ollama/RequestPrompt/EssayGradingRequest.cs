namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for AI Essay Grading
    /// </summary>
    public class EssayGradingRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The submission ID being graded
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// The question ID being graded
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// The original question text
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// The student's essay answer to grade
        /// </summary>
        public string StudentAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Maximum points for this question
        /// </summary>
        public int MaxPoints { get; set; }

        /// <summary>
        /// Grading rubric or criteria (optional)
        /// </summary>
        public string GradingRubric { get; set; } = string.Empty;

        /// <summary>
        /// Model/ideal answer for comparison (optional)
        /// </summary>
        public string ModelAnswer { get; set; } = string.Empty;
    }
}
