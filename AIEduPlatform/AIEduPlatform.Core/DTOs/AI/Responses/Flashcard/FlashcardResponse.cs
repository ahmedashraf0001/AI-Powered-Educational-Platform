using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Flashcard
{
    /// <summary>
    /// A single flashcard as returned by AI - matches the Required JSON Response Format in prompt
    /// </summary>
    public class FlashcardItem
    {
        /// <summary>
        /// Front of the flashcard (question/term)
        /// </summary>
        [JsonPropertyName("front")]
        public string Front { get; set; } = string.Empty;

        /// <summary>
        /// Back of the flashcard (answer/definition)
        /// </summary>
        [JsonPropertyName("back")]
        public string Back { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty level: "easy", "medium", "hard"
        /// </summary>
        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Title of the source material
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Location within the source (page number, timestamp, etc.)
        /// </summary>
        [JsonPropertyName("sourceLocation")]
        public string SourceLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full response wrapper for Flashcard Generation
    /// </summary>
    public class FlashcardResponse : ResponseBase
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
        /// The generated flashcards (parsed from AI JSON array response)
        /// </summary>
        public List<FlashcardItem> Flashcards { get; set; } = new List<FlashcardItem>();
    }
}
