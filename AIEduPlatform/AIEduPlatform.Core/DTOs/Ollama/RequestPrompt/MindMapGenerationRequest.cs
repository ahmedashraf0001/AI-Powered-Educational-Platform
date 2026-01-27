namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for Mind Map Generation
    /// </summary>
    public class MindMapGenerationRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The central topic for the mind map
        /// </summary>
        public string CentralTopic { get; set; } = string.Empty;

        /// <summary>
        /// Maximum depth of the mind map (levels of branches)
        /// </summary>
        public int MaxDepth { get; set; } = 3;
    }
}
