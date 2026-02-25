namespace AIEduPlatform.Core.DTOs.Exams
{
    public record ExamDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
        public int QuestionCount { get; init; }
    }
}
