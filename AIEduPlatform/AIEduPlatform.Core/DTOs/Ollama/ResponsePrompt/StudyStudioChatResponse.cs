namespace AIEduPlatform.Core.DTOs.Ollama.ResponsePrompt
{
    /// <summary>
    /// Response for Study Studio Chat
    /// </summary>
    public class StudyStudioChatResponse : StructuredPromptResponse
    {
        /// <summary>
        /// The session ID for continuity
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// Suggested follow-up questions
        /// </summary>
        public List<string> SuggestedFollowUps { get; set; } = new List<string>();
    }
}
