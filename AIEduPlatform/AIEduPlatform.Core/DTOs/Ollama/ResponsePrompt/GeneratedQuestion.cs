namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// A generated exam question
    /// </summary>
    public class GeneratedQuestion
    {
        /// <summary>
        /// The question text
        /// </summary>
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Type: "mcq", "true_false", "short_answer", "essay"
        /// </summary>
        public string QuestionType { get; set; } = string.Empty;

        /// <summary>
        /// Options for MCQ (null for other types)
        /// </summary>
        public List<string>? Options { get; set; }

        /// <summary>
        /// Correct answer (or model answer for essays)
        /// </summary>
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Explanation for the correct answer
        /// </summary>
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty: "easy", "medium", "hard"
        /// </summary>
        public string Difficulty { get; set; } = string.Empty;

        /// <summary>
        /// Suggested points for this question
        /// </summary>
        public int SuggestedPoints { get; set; }

        /// <summary>
        /// Source references for this question
        /// </summary>
        public List<SourceReference> Sources { get; set; } = new List<SourceReference>();
    }
}
