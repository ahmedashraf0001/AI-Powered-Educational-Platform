namespace AIEduPlatform.Core.DTOs.Exams
{
    /// <summary>
    /// DTO for exam attempt (timer persistence)
    /// </summary>
    public record ExamAttemptDto
    {
        public Guid Id { get; init; }
        public Guid ExamId { get; init; }
        public Guid StudentId { get; init; }
        public DateTime StartedAt { get; init; }
        public bool IsSubmitted { get; init; }
        public int RemainingSeconds { get; init; }
        public Dictionary<string, string>? SavedAnswers { get; init; }
    }
}
