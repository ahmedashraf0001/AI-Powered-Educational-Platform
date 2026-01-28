using AIEduPlatform.Core.DTOs.AI.Common;

namespace AIEduPlatform.Core.DTOs.AI.Requests.Quiz
{
    /// <summary>
    /// Request for Practice Quiz Generation in Study Studio.
    /// Maps to PromptBuilder.BuildQuizPrompt parameters.
    /// </summary>
    public class QuizRequest : RequestBase
    {
        /// <summary>
        /// The study session ID for tracking
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic for the quiz
        /// Maps to: topic parameter in BuildQuizPrompt
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// Number of questions to generate
        /// Maps to: numberOfQuestions parameter in BuildQuizPrompt
        /// </summary>
        public int NumberOfQuestions { get; set; } = 5;

        /// <summary>
        /// Difficulty level: "easy", "medium", "hard"
        /// Maps to: difficulty parameter in BuildQuizPrompt
        /// </summary>
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Types of questions to include: "mcq", "true_false", "short_answer"
        /// Maps to: questionTypes parameter in BuildQuizPrompt
        /// </summary>
        public List<string> QuestionTypes { get; set; } = new List<string> { "mcq" };

        /// <summary>
        /// Optional conversation history for contextual generation
        /// Maps to: conversationHistory parameter in BuildQuizPrompt
        /// </summary>
        public List<AiChatMessage>? ConversationHistory { get; set; }
    }
}
