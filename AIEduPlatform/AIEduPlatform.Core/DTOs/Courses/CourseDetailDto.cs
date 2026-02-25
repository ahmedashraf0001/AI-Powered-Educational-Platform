namespace AIEduPlatform.Core.DTOs.Courses
{
    public record CourseDetailDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid TeacherId { get; init; }
        public string TeacherName { get; init; } = string.Empty;
        public bool IsPublished { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public int LectureCount { get; init; }
        public int EnrollmentCount { get; init; }
        public bool IsEnrolled { get; init; }
        public bool HasReviewed { get; init; }
        public double AverageRating { get; init; }
        public int ReviewCount { get; init; }
        public List<LectureSummaryDto> Lectures { get; init; } = [];
    }

    public record LectureSummaryDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
