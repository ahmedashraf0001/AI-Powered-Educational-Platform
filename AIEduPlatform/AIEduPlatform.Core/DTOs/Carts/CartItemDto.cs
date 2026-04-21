namespace AIEduPlatform.Core.DTOs.Carts
{
    public record CartItemDto
    {
        private string? _courseThumbnailUrl;

        public Guid CartItemId { get; init; }
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string? CourseThumbnailUrl
        {
            get => _courseThumbnailUrl;
            init => _courseThumbnailUrl = NormalizePath(value);
        }
        public string TeacherName { get; init; } = string.Empty;
        public decimal OriginalPrice { get; init; }
        public decimal PriceAtTimeOfAdding { get; init; }

        private static string? NormalizePath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return path;

            return path.Replace(" ", "%20");
        }
    }
}
