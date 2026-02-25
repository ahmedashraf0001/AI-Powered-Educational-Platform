namespace AIEduPlatform.Core.DTOs.StudySessions
{
    public record SessionSummaryDto
    {
        public Guid Id { get; init; }
        public Guid CourseId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public DateTime StartedAt { get; init; }
        public DateTime LastActivity { get; init; }
    }
}
