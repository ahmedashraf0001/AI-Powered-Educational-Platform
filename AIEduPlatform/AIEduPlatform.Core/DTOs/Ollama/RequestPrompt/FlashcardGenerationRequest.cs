namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for Flashcard Generation
    /// </summary>
    public class FlashcardGenerationRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic for flashcards
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Number of flashcards to generate
        /// </summary>
        public int NumberOfFlashcards { get; set; } = 10;
    }
}
