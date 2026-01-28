using System.Text.Json.Serialization;

namespace AIEduPlatform.Core.DTOs.AI.Responses.QuestionGeneration
{
    /// <summary>
    /// Rubric criterion for essay questions - matches the Required JSON Response Format in prompt
    /// </summary>
    public class RubricCriterion
    {
        /// <summary>
        /// Name of the criterion
        /// </summary>
        [JsonPropertyName("criterion")]
        public string Criterion { get; set; } = string.Empty;

        /// <summary>
        /// Maximum points for this criterion
        /// </summary>
        [JsonPropertyName("maxPoints")]
        public int MaxPoints { get; set; }

        /// <summary>
        /// Description of what this criterion assesses
        /// </summary>
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// An exam question as returned by AI - matches the Required JSON Response Format in prompt
    /// Extended from quiz questions with additional exam-specific fields
    /// </summary>
    public class ExamQuestion
    {
        /// <summary>
        /// The complete question text
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
        /// </summary>
        [JsonPropertyName("options")]
        public List<string>? Options { get; set; }

        /// <summary>
        /// Correct answer (or expected answer for short_answer)
        /// </summary>
        [JsonPropertyName("correctAnswer")]
        public string CorrectAnswer { get; set; } = string.Empty;

        /// <summary>
        /// Detailed explanation of why this is correct and why others are wrong
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
        /// Grading criteria description
        /// </summary>
        [JsonPropertyName("gradingCriteria")]
        public string GradingCriteria { get; set; } = string.Empty;

        /// <summary>
        /// Title of the source material
        /// </summary>
        [JsonPropertyName("sourceTitle")]
        public string SourceTitle { get; set; } = string.Empty;

        /// <summary>
        /// Section within the source material
        /// </summary>
        [JsonPropertyName("sourceSection")]
        public string? SourceSection { get; set; }

        /// <summary>
        /// Location within the source material (page, timestamp, etc.)
        /// </summary>
        [JsonPropertyName("sourceLocation")]
        public string SourceLocation { get; set; } = string.Empty;

        /// <summary>
        /// What skill/knowledge this question assesses
        /// </summary>
        [JsonPropertyName("learningObjective")]
        public string LearningObjective { get; set; } = string.Empty;

        /// <summary>
        /// Model answer for essay questions (comprehensive answer for full marks)
        /// </summary>
        [JsonPropertyName("modelAnswer")]
        public string? ModelAnswer { get; set; }

        /// <summary>
        /// Grading rubric for essay questions
        /// </summary>
        [JsonPropertyName("gradingRubric")]
        public List<RubricCriterion>? GradingRubric { get; set; }
    }

    /// <summary>
    /// Full response wrapper for Exam Question Generation
    /// </summary>
    public class QuestionGenerationResponse : ResponseBase
    {
        /// <summary>
        /// The exam ID this generation was for
        /// </summary>
        public Guid ExamId { get; set; }

        /// <summary>
        /// The generated exam questions (parsed from AI JSON array response)
        /// </summary>
        public List<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    }
}
