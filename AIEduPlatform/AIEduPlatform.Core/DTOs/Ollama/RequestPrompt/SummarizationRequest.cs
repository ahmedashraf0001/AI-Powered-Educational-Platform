namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for Content Summarization
    /// </summary>
    public class SummarizationRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The material ID to summarize
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Desired summary length: "brief", "moderate", "detailed"
        /// </summary>
        public string SummaryLength { get; set; } = "moderate";

        /// <summary>
        /// Include key points as bullet list
        /// </summary>
        public bool IncludeKeyPoints { get; set; } = true;
    }
}
