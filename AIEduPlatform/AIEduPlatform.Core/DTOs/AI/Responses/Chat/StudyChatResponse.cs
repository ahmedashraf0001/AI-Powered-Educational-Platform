using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Chat
{
    /// <summary>
    /// Response for Study Studio Chat - matches the expected conversational response
    /// </summary>
    public class ChatResponseData
    {
        /// <summary>
        /// The AI's response message content
        /// </summary>
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Suggested follow-up questions for the student
        /// </summary>
        [JsonPropertyName("suggestedFollowUps")]
        public List<string>? SuggestedFollowUps { get; set; }
    }

    /// <summary>
    /// Full response wrapper for Study Chat
    /// </summary>
    public class StudyChatResponse : ResponseBase
    {
        /// <summary>
        /// The session ID for continuity
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The AI's response content
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Suggested follow-up questions
        /// </summary>
        public List<string> SuggestedFollowUps { get; set; } = new List<string>();
    }
}
