namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// A node in the mind map
    /// </summary>
    public class MindMapNode
    {
        /// <summary>
        /// Unique ID for this node
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The label/text of this node
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Optional description or details
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Child nodes (branches)
        /// </summary>
        public List<MindMapNode> Children { get; set; } = new List<MindMapNode>();

        /// <summary>
        /// Source reference for this node
        /// </summary>
        public SourceReference? Source { get; set; }
    }
}
