using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Payments
{
    public record OrderStatusDto
    {
        public Guid OrderId { get; init; }
        public OrderStatus Status { get; init; }
        public DateTime? PaidAt { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = "usd";
        public List<EnrolledCourseInfoDto> EnrolledCourses { get; init; } = [];
    }

    public record EnrolledCourseInfoDto
    {
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}
