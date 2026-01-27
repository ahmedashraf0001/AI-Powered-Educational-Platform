namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for AI Question Generation
    /// </summary>
    public class QuestionGenerationRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The exam ID this generation is for
        /// </summary>
        public Guid ExamId { get; set; }

        /// <summary>
        /// Number of questions to generate
        /// </summary>
        public int NumberOfQuestions { get; set; } = 10;

        /// <summary>
        /// Types of questions to generate (e.g., "mcq", "true_false", "short_answer", "essay")
        /// </summary>
        public List<string> QuestionTypes { get; set; } = new List<string>();

        /// <summary>
        /// Difficulty level: "easy", "medium", "hard", or "mixed"
        /// </summary>
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Specific topics to focus on (optional)
        /// </summary>
        public List<string> FocusTopics { get; set; } = new List<string>();
    }
}
