namespace AIEduPlatform.Core.DTOs.Courses
{
    public record CourseDetailDto
    {
        private string? _thumbnailUrl;

        public Guid CourseId { get; init; }
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
        public Guid? CategoryId { get; init; }
        public string? CategoryName { get; init; }
        public decimal Price { get; init; }
        public bool IsFree { get; init; }
        public string? ThumbnailUrl
        {
            get => _thumbnailUrl;
            init => _thumbnailUrl = NormalizePath(value);
        }
        public List<LectureSummaryDto> Lectures { get; init; } = [];

        private static string? NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            return path.Replace(" ", "%20");
        }
    }

    public record LectureSummaryDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
        public int MaterialCount { get; init; }
    }
}
