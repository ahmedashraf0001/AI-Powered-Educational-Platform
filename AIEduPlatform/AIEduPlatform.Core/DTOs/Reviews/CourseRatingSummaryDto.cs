namespace AIEduPlatform.Core.DTOs.Reviews
{
    public record CourseRatingSummaryDto
    {
        public Guid CourseId { get; init; }
        public double AverageRating { get; init; }
        public int TotalReviews { get; init; }
        public int[] RatingDistribution { get; init; } = new int[5]; // index 0 = 1-star, index 4 = 5-star
    }
}
