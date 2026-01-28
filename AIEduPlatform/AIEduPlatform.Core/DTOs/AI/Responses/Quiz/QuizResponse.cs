using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.Quiz
{
    /// <summary>
    /// A quiz question as returned by AI - matches the Required JSON Response Format in prompt
    /// Used for both practice quizzes (Study Studio) and exam question generation
    /// </summary>
    public class QuizQuestion
    {
        /// <summary>
        /// The question text
        /// </summary>
        [JsonPropertyName("questionText")]
        public string QuestionText { get; set; } = string.Empty;

        /// <summary>
        /// Type: "mcq", "true_false", "short_answer", "essay"
        /// </summary>
        [JsonPropertyName("questionType")]
        public string QuestionType { get; set; } = "mcq";

        /// <summary>
        /// Options for MCQ (null for other types)
        /// For true_false: ["True", "False"]
        /// </summary>
        [JsonPropertyName("options")]
        public List<string>? Options { get; set; }

        /// <summary>
        /// Correct answer (or expected answer for short_answer)
        /// </summary>
        [JsonPropertyName("correctAnswer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Explanation of why this is correct
        /// </summary>
        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;

        /// <summary>
        /// Difficulty: "easy", "medium", "hard"
        /// </summary>
        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; } = "medium";

        /// <summary>
        /// Suggested points for this question
        /// </summary>
        [JsonPropertyName("suggestedPoints")]
        public int SuggestedPoints { get; set; } = 1;

        /// <summary>
        /// Title of the source material
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Location within the source material
        /// </summary>
        [JsonPropertyName("sourceLocation")]
        public string SourceLocation { get; set; } = string.Empty;
    }

    /// <summary>
    /// Full response wrapper for Quiz Generation (Study Studio)
    /// </summary>
    public class QuizResponse : ResponseBase
    {
        /// <summary>
        /// The study session ID
        /// </summary>
        public Guid SessionId { get; set; }

        /// <summary>
        /// The topic of the quiz
        /// </summary>
        public string Topic { get; set; } = string.Empty;

        /// <summary>
        /// The generated quiz questions (parsed from AI JSON array response)
        /// </summary>
        public List<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
