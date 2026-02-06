namespace AIEduPlatform.Core.DTOs.Exams
{
    public record ExamDetailDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
        public List<QuestionDto> Questions { get; init; } = [];
        public int SubmissionCount { get; init; }
    }
}
