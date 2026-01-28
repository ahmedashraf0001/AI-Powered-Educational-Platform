using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Common
{
    /// <summary>
    /// Token usage statistics from AI response
    /// </summary>
    public class AiTokenUsage
    {
        /// <summary>
        /// Number of tokens in the prompt
        /// </summary>
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        /// <summary>
        /// Number of tokens in the completion/response
        /// </summary>
        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        /// <summary>
        /// Total tokens used (prompt + completion)
        /// </summary>
        [JsonPropertyName("total_tokens")]
        public int TotalTokens => PromptTokens + CompletionTokens;
    }
}
