using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.MindMap
{
    /// <summary>
    /// A mind map node as returned by AI - matches the Required JSON Response Format in prompt
    /// </summary>
    public class MindMapNode
    {
        /// <summary>
        /// Unique identifier for this node
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// The label/text of this node (1-5 words)
        /// </summary>
        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Brief description or details about this node
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Title of the source material (optional for child nodes)
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string? SourceTitle { get; set; }

        /// <summary>
        /// Location within the source material (optional)
        /// </summary>
        [JsonPropertyName("sourceLocation")]
        public string? SourceLocation { get; set; }

        /// <summary>
        /// Child nodes (branches) of this node
        /// </summary>
        [JsonPropertyName("children")]
        public List<MindMapNode> Children { get; set; } = new List<MindMapNode>();
    }

    /// <summary>
    /// Full response wrapper for Mind Map Generation
    /// </summary>
    public class MindMapResponse : ResponseBase
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
        /// The root node of the mind map (parsed from AI JSON response)
        /// </summary>
        public MindMapNode RootNode { get; set; } = new MindMapNode();
    }
}
