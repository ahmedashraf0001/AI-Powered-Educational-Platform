namespace AIEduPlatform.Core.DTOs.AI.Requests.Summarization
{
    /// <summary>
    /// Request for Content Summarization.
    /// Maps to PromptBuilder.BuildSummarizationPrompt parameters.
    /// </summary>
    public class SummarizationRequest : RequestBase
    {
        /// <summary>
        /// The material ID to summarize (for tracking)
        /// </summary>
        public Guid MaterialId { get; set; }

        /// <summary>
        /// Summary length in words (approximate target)
        /// Maps to: summaryLength parameter in BuildSummarizationPrompt
        /// Use values like: 150 (brief), 400 (moderate), 800 (detailed)
        /// </summary>
        public int SummaryLength { get; set; } = 400;

        /// <summary>
        /// Include key points as bullet list
        /// Maps to: includeKeyPoints parameter in BuildSummarizationPrompt
        /// </summary>
        public bool IncludeKeyPoints { get; set; } = true;
    }
}
