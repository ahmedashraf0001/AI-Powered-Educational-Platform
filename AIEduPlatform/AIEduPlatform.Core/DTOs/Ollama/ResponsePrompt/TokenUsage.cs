namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Token usage statistics
    /// </summary>
    public class TokenUsage
    {
        /// <summary>
        /// Number of tokens in the prompt
        /// </summary>
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens in the response
        /// </summary>
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total tokens used
        /// </summary>
        public int TotalTokens => PromptTokens + CompletionTokens;
    }
}
