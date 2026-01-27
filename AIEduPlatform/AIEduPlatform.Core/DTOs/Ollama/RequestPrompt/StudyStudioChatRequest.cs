namespace AIEduPlatform.Core.DTOs.Ollama.RequestPrompt
{
    /// <summary>
    /// Request for Study Studio Chat feature
    /// </summary>
    public class StudyStudioChatRequest : StructuredPromptRequest
    {
        /// <summary>
        /// The course ID for context
        /// </summary>
        public Guid CourseId { get; set; }

        /// <summary>
        /// The student's study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Previous conversation history for context continuity
        /// </summary>
        public List<ChatMessage> ConversationHistory { get; set; } = new List<ChatMessage>();
    }
}
