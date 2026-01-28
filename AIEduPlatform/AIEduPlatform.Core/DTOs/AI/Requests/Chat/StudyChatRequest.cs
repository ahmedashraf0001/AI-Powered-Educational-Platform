using AIEduPlatform.Core.DTOs.AI.Common;

namespace AIEduPlatform.Core.DTOs.AI.Requests.Chat
{
    /// <summary>
    /// Request for Study Studio Chat feature.
    /// Maps to PromptBuilder.BuildStudyChatPrompt parameters.
    /// </summary>
    public class StudyChatRequest : RequestBase
    {
        /// <summary>
        /// The course ID for context retrieval
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// The student's study session ID for tracking
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The student's current question
        /// Maps to: userQuestion parameter in BuildStudyChatPrompt
        /// </summary>
        public string UserQuestion { get; set; } = string.Empty;

        /// <summary>
        /// Previous conversation history for context continuity
        /// Maps to: conversationHistory parameter in BuildStudyChatPrompt
        /// </summary>
        public List<AiChatMessage> ConversationHistory { get; set; } = new List<AiChatMessage>();
    }
}
