using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Summarization
{
    /// <summary>
    /// Summarization response as returned by AI - matches the Required JSON Response Format in prompt
    /// </summary>
    public class SummarizationData
    {
        /// <summary>
        /// The main summary text as clear paragraphs
        /// </summary>
        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Key points extracted from the content
        /// </summary>
        [JsonPropertyName("keyPoints")]
        public List<string> KeyPoints { get; set; } = new List<string>();

        /// <summary>
        /// Key terms and their definitions
        /// </summary>
        [JsonPropertyName("keyTerms")]
        public Dictionary<string, string> KeyTerms { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Title of the summarized material
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Approximate word count of original content
        /// </summary>
        [JsonPropertyName("originalLength")]
        public string OriginalLength { get; set; } = string.Empty;

        /// <summary>
        /// Word count of the summary
        /// </summary>
        [JsonPropertyName("summaryLength")]
        public string SummaryLength { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full response wrapper for Summarization
    /// </summary>
    public class SummarizationResponse : ResponseBase
    {
        /// <summary>
        /// The material ID that was summarized
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// The parsed summarization data from AI
        /// </summary>
        public SummarizationData Data { get; set; } = new SummarizationData();
    }
}
