namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Content Summarization
    /// </summary>
    public class SummarizationResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The material ID that was summarized
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// The summary text
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Key points extracted
        /// </summary>
        public List<string> KeyPoints { get; set; } = new List<string>();

        /// <summary>
        /// Key terms and definitions
        /// </summary>
        public Dictionary<string, string> KeyTerms { get; set; } = new Dictionary<string, string>();
    }
}
