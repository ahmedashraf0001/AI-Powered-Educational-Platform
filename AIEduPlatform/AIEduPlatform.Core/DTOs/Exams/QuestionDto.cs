using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Exams
{
    public record QuestionDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public QuestionType Type { get; init; }
        public string Text { get; init; } = string.Empty;
        public string Options { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public int Points { get; init; }
        public int Order { get; init; }
    }
}
