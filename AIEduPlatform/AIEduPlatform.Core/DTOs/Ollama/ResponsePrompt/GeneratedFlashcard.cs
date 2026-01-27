namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// A generated flashcard
    /// </summary>
    public class GeneratedFlashcard
    {
        /// <summary>
        /// Front of the flashcard (question/term)
        /// </summary>
        public string Front { get; set; } = string.Empty;

        /// <summary>
        /// Back of the flashcard (answer/definition)
        /// </summary>
        public string Back { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty level
        /// </summary>
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Source reference
        /// </summary>
        public SourceReference? Source { get; set; }
    }
}
