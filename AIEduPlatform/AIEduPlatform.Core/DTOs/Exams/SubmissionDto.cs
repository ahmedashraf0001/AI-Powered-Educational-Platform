namespace AIEduPlatform.Core.DTOs.Exams
{
    public record SubmissionDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public Guid StudentId { get; init; }
        public string ExamTitle { get; init; } = string.Empty;
        public string CourseName { get; init; } = string.Empty;
        public string StudentName { get; init; } = string.Empty;
        public DateTime SubmittedAt { get; init; }
        public bool IsGraded { get; init; }
        public float? Score { get; init; }
    }
}
