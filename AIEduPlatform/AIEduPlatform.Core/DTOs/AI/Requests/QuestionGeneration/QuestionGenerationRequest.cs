namespace AIEduPlatform.Core.DTOs.AI.Requests.QuestionGeneration
{
    /// <summary>
    /// Request for AI Exam Question Generation.
    /// Maps to PromptBuilder.BuildQuestionGenerationPrompt parameters.
    /// </summary>
    public class QuestionGenerationRequest : RequestBase
    {
        /// <summary>
        /// The exam ID this generation is for (for tracking)
        /// </summary>
        public Guid ExamId { get; set; }

        /// <summary>
        /// Number of questions to generate
        /// Maps to: numberOfQuestions parameter in BuildQuestionGenerationPrompt
        /// </summary>
        public int NumberOfQuestions { get; set; } = 10;

        /// <summary>
        /// Difficulty level: "easy", "medium", "hard", or "mixed"
        /// Maps to: difficulty parameter in BuildQuestionGenerationPrompt
        /// </summary>
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Types of questions to generate: "mcq", "true_false", "short_answer", "essay"
        /// Maps to: questionTypes parameter in BuildQuestionGenerationPrompt
        /// </summary>
        public List<string> QuestionTypes { get; set; } = new List<string> { "mcq" };

        /// <summary>
        /// Specific topics to focus on (optional)
        /// If null/empty, all topics from materials will be covered
        /// Maps to: focusTopics parameter in BuildQuestionGenerationPrompt
        /// </summary>
        public List<string>? FocusTopics { get; set; }
    }
}
