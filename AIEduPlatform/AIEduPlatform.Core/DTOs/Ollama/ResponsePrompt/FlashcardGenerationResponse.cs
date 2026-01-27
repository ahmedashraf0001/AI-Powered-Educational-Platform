namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Flashcard Generation
    /// </summary>
    public class FlashcardGenerationResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic of the flashcards
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// The generated flashcards
        /// </summary>
        public List<GeneratedFlashcard> Flashcards { get; set; } = new List<GeneratedFlashcard>();
    }
}
