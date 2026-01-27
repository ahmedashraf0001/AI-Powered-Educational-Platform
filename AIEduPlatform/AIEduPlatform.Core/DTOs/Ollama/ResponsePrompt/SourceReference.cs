namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Reference to a source used in the response
    /// </summary>
    public class SourceReference
    {
        /// <summary>
        /// Title of the source material
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Page number or timestamp
        /// </summary>
        public string PageOrTimestamp { get; set; } = string.Empty;

        /// <summary>
        /// Section name
        /// </summary>
        public string Section { get; set; } = string.Empty;

        /// <summary>
        /// Material ID for linking
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Relevance score of this source
        /// </summary>
        public float RelevanceScore { get; set; }
    }
}
