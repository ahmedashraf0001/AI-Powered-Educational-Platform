using AIEduPlatform.Core.DTOs.AI.Common;

namespace AIEduPlatform.Core.DTOs.AI.Requests.MindMap
{
    /// <summary>
    /// Request for Mind Map Generation.
    /// Maps to PromptBuilder.BuildMindMapPrompt parameters.
    /// </summary>
    public class MindMapRequest : RequestBase
    {
        /// <summary>
        /// The study session ID for tracking
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The central topic for the mind map
        /// Maps to: centralTopic parameter in BuildMindMapPrompt
        /// </summary>
        public string CentralTopic { get; set; } = string.Empty;

        /// <summary>
        /// Maximum depth of the mind map (levels of branches)
        /// Maps to: maxDepth parameter in BuildMindMapPrompt
        /// Default is 3 as per PromptBuilder
        /// </summary>
        public int MaxDepth { get; set; } = 3;

        /// <summary>
        /// Optional conversation history for contextual generation
        /// Maps to: conversationHistory parameter in BuildMindMapPrompt
        /// </summary>
        public List<AiChatMessage>? ConversationHistory { get; set; }
    }
}
