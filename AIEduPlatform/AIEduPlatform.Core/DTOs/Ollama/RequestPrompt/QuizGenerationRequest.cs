namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for Quiz Generation in Study Studio
    /// </summary>
    public class QuizGenerationRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic for the quiz
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Number of questions
        /// </summary>
        public int NumberOfQuestions { get; set; } = 5;

        /// <summary>
        /// Difficulty: "easy", "medium", "hard"
        /// </summary>
        public string Difficulty { get; set; } = "medium";
    }
}
