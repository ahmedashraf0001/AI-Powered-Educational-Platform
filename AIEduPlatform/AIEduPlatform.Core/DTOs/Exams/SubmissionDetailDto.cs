using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Exams
{
    public record SubmissionDetailDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public Guid StudentId { get; init; }
        public string ExamTitle { get; init; } = string.Empty;
        public string CourseName { get; init; } = string.Empty;
        public string StudentName { get; init; } = string.Empty;
        public List<SubmissionAnswerDto> Answers { get; init; } = [];
        public DateTime SubmittedAt { get; init; }
        public GradeDto? Grade { get; init; }
    }

    public record SubmissionAnswerDto
    {
        public Guid QuestionId { get; init; }
        public string QuestionText { get; init; } = string.Empty;
        public QuestionType QuestionType { get; init; }
        public string Answer { get; init; } = string.Empty;
        public string CorrectAnswer { get; init; } = string.Empty;
        public List<string> Options { get; init; } = [];
        public int Points { get; init; }
        public int Order { get; init; }
    }
}
