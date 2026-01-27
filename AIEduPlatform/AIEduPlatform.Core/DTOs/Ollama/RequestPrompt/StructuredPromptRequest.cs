using AIEduPlatform.Core.DTOs.Ollama.chunks;
using System;
using System.Collections.Generic;

namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Base structured prompt request following the pattern:
    /// 1. Instructions (system context)
    /// 2. Context (relevant chunks with metadata)
    /// 3. User prompt
    /// </summary>
    public class StructuredPromptRequest
    {
        /// <summary>
        /// The system instructions that define the AI's behavior and role
        /// </summary>
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// List of relevant context chunks retrieved from course materials
        /// </summary>
        public List<ContextChunk> ContextChunks { get; set; } = new List<ContextChunk>();

        /// <summary>
        /// The user's actual prompt/question
        /// </summary>
        public string UserPrompt { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Temperature for response generation (0.0 to 1.0)
        /// Lower = more deterministic, Higher = more creative
        /// </summary>
        public float Temperature { get; set; } = 0.7f;

        /// <summary>
        /// Optional: Maximum tokens to generate in response
        /// </summary>
        public int MaxTokens { get; set; } = 2048;

        /// <summary>
        /// Optional: Whether to stream the response
        /// </summary>
        public bool Stream { get; set; } = false;
    }
}
