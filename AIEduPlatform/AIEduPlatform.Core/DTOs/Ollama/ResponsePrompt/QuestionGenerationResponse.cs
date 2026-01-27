namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Question Generation
    /// </summary>
    public class QuestionGenerationResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The generated questions
        /// </summary>
        public List<GeneratedQuestion> Questions { get; set; } = new List<GeneratedQuestion>();
    }
}
