namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Mind Map Generation
    /// </summary>
    public class MindMapGenerationResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The central topic
        /// </summary>
        public string CentralTopic { get; set; } = string.Empty;

        /// <summary>
        /// The mind map nodes
        /// </summary>
        public MindMapNode RootNode { get; set; } = new MindMapNode();
    }
}
