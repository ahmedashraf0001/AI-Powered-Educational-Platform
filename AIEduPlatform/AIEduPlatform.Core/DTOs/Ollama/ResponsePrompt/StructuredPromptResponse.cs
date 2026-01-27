using System;
using System.Collections.Generic;

namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Base response from structured prompt requests
    /// </summary>
    public class StructuredPromptResponse
    {
        /// <summary>
        /// The generated text response from the AI
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Sources used to generate this response (for citation)
        /// </summary>
        public List<SourceReference> Sources { get; set; } = new List<SourceReference>();

        /// <summary>
        /// Whether the request was successful
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Error message if the request failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Token usage statistics
        /// </summary>
        public TokenUsage TokenUsage { get; set; } = new TokenUsage();

        /// <summary>
        /// Model used for generation
        /// </summary>
        public string Model { get; set; } = "qwen3:8b";

        /// <summary>
        /// Time taken to generate the response in milliseconds
        /// </summary>
        public long GenerationTimeMs { get; set; }
    }
}
