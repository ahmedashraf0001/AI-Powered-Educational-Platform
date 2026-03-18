namespace AIEduPlatform.Core.DTOs.Courses
{
    public record CourseListDto
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid TeacherId { get; init; }
        public string TeacherName { get; init; } = string.Empty;
        public bool IsPublished { get; init; }
        public int LectureCount { get; init; }
        public int EnrollmentCount { get; init; }
        public DateTime CreatedAt { get; init; }
        public bool IsEnrolled { get; init; }
        public double AverageRating { get; init; }
        public int ReviewCount { get; init; }
        public Guid? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public decimal Price { get; init; }
        public bool IsFree { get; init; }
        public string? ThumbnailUrl { get; init; }
    }
}
