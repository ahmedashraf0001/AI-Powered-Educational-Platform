namespace AIEduPlatform.Core.DTOs.Exams
{
    public record SubmissionDetailDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public Guid StudentId { get; init; }
        public string Answers { get; init; } = string.Empty;
        public DateTime SubmittedAt { get; init; }
        public GradeDto? Grade { get; init; }
    }
}
