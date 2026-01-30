using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.DTOs.AI.Requests
{
    /// <summary>
    /// Base request for all AI prompt operations.
    /// Maps to PromptBuilder's common parameters: instructions, contextChunks, userPrompt
    /// </summary>
    public abstract class RequestBase
    {
        /// <summary>
        /// The system instructions that define the AI's behavior and role
        /// If not provided, default instructions from prompt templates will be used
        /// </summary>
        public string? Instructions { get; set; }

        /// <summary>
        /// List of relevant context chunks retrieved from course materials via RAG
        /// </summary>
        public List<ContextChunk> ContextChunks { get; set; } = new List<ContextChunk>();

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

        /// <summary>
        /// Optional: Model to use for generation
        /// </summary>
        public string? Model { get; set; }
    }
}
