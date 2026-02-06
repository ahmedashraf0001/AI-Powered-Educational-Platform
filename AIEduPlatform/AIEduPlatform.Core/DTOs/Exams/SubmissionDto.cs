namespace AIEduPlatform.Core.DTOs.Exams
{
    public record SubmissionDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public Guid StudentId { get; init; }
        public DateTime SubmittedAt { get; init; }
        public bool IsGraded { get; init; }
    }
}
