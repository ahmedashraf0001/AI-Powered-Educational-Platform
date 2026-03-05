using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Courses
{
    public record EnrollmentDto
    {
        public Guid Id { get; init; }
        public Guid StudentId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public DateTime EnrolledAt { get; init; }
        public EnrollmentStatus Status { get; init; }
        public double ProgressPercentage { get; init; }
        public int CompletedLectures { get; init; }
        public int TotalLectures { get; init; }
        public DateTime? LastAccessedAt { get; init; }
        public bool IsCompleted { get; init; }

        // Revamped enrollment fields
        public Guid? OrderId { get; init; }
        public decimal AmountPaid { get; init; }
        public DateTime? RefundedAt { get; init; }
        public decimal? RefundAmount { get; init; }
        public string? StripeRefundId { get; init; }
        public DateTime? UnenrolledAt { get; init; }
    }
}
