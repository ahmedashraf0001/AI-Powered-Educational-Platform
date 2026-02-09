using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record GeneratedQuizDto
    {
        public Guid Id { get; init; }
        public string Topic { get; init; } = string.Empty;
        public QuizDifficulty Difficulty { get; init; }
        public string Questions { get; init; } = string.Empty;
        public string? StudentAnswers { get; init; }
        public float Score { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
