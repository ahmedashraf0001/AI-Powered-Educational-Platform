namespace AIEduPlatform.Core.DTOs.Courses
{
    public record CourseListDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid TeacherId { get; init; }
        public bool IsPublished { get; init; }
        public int LectureCount { get; init; }
        public int EnrollmentCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
