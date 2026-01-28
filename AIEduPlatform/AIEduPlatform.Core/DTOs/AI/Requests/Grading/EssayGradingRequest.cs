namespace AIEduPlatform.Core.DTOs.AI.Requests.Grading
{
    /// <summary>
    /// Request for AI Essay Grading.
    /// Maps to PromptBuilder.BuildEssayGradingPrompt parameters.
    /// </summary>
    public class EssayGradingRequest : RequestBase
    {
        /// <summary>
        /// The submission ID being graded (for tracking)
        /// </summary>
        public Guid SubmissionId { get; set; }

        /// <summary>
        /// The question ID being graded (for tracking)
        /// </summary>
        public Guid QuestionId { get; set; }

        /// <summary>
        /// The original question text
        /// Maps to: questionText parameter in BuildEssayGradingPrompt
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Maximum points for this question
        /// Maps to: maxPoints parameter in BuildEssayGradingPrompt
        /// </summary>
        public int MaxPoints { get; set; }

        /// <summary>
        /// Grading rubric or criteria (optional)
        /// If not provided, default rubric from EssayGradingPrompts will be used
        /// Maps to: gradingRubric parameter in BuildEssayGradingPrompt
        /// </summary>
        public string? GradingRubric { get; set; }

        /// <summary>
        /// Model/ideal answer for comparison (optional)
        /// Maps to: modelAnswer parameter in BuildEssayGradingPrompt
        /// </summary>
        public string? ModelAnswer { get; set; }

        /// <summary>
        /// The student's essay answer to grade
        /// Maps to: studentAnswer parameter in BuildEssayGradingPrompt
        /// </summary>
        public string StudentAnswer { get; set; } = string.Empty;
    }
}
