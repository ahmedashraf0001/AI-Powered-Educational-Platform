namespace AIEduPlatform.Core.DTOs.AI.Requests.Flashcard
{
    /// <summary>
    /// Request for Flashcard Generation.
    /// Maps to PromptBuilder.BuildFlashCardPrompt parameters.
    /// </summary>
    public class FlashcardRequest : RequestBase
    {
        /// <summary>
        /// The study session ID for tracking
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic for flashcards
        /// Maps to: topic parameter in BuildFlashCardPrompt
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Number of flashcards to generate
        /// Maps to: numOfCards parameter in BuildFlashCardPrompt
        /// </summary>
        public int NumberOfCards { get; set; } = 10;
    }
}
