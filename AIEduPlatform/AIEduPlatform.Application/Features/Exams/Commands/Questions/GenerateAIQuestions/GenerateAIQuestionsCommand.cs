using AIEduPlatform.Core.Domain.Enums;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.GenerateAIQuestions
{
    /// <summary>
    /// Command for teachers to generate exam questions using AI based on course content
    /// </summary>
    public record GenerateAIQuestionsCommand : IRequest<GenerateAIQuestionsResult>
    {
        /// <summary>
        /// The exam to add generated questions to
        /// </summary>
        public Guid ExamId { get; init; }

        /// <summary>
        /// Number of questions to generate
        /// </summary>
        public int NumberOfQuestions { get; init; } = 5;

        /// <summary>
        /// Difficulty level: "easy", "medium", "hard"
        /// </summary>
        public string Difficulty { get; init; } = "medium";

        /// <summary>
        /// Types of questions to generate
        /// </summary>
        public List<QuestionType> QuestionTypes { get; init; } = new();

        /// <summary>
        /// Optional: Specific topics to focus on
        /// </summary>
        public List<string>? FocusTopics { get; init; }

        /// <summary>
        /// Optional: Specific lecture IDs to use as source material
        /// </summary>
        public List<Guid>? LectureIds { get; init; }

        /// <summary>
        /// Optional: Specific material IDs to use as source material
        /// </summary>
        public List<Guid>? MaterialIds { get; init; }
    }

    /// <summary>
    /// Result of AI question generation
    /// </summary>
    public record GenerateAIQuestionsResult
    {
        /// <summary>
        /// Whether generation was successful
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// IDs of the generated questions
        /// </summary>
        public List<Guid> QuestionIds { get; init; } = new();

        /// <summary>
        /// Number of questions generated
        /// </summary>
        public int QuestionsGenerated { get; init; }

        /// <summary>
        /// Error message if generation failed
        /// </summary>
        public string? Error { get; init; }
    }
}
