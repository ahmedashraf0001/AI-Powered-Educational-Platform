namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Quiz Generation in Study Studio
    /// </summary>
    public class QuizGenerationResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic of the quiz
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// The generated quiz questions
        /// </summary>
        public List<GeneratedQuestion> Questions { get; set; } = new List<GeneratedQuestion>();
    }
}
