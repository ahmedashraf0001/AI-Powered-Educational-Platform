namespace AIEduPlatform.Core.DTOs.Carts
{
    public record CartItemDto
    {
        public Guid CartItemId { get; init; }
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public string? CourseThumbnailUrl { get; init; }
        public string TeacherName { get; init; } = string.Empty;
        public decimal OriginalPrice { get; init; }
        public decimal PriceAtTimeOfAdding { get; init; }
    }
}
