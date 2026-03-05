namespace AIEduPlatform.Core.DTOs.Enrollments
{
    public record UnenrollmentResultDto
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;
        public decimal? RefundAmount { get; init; }
        public string? RefundCurrency { get; init; }
        public string? RefundEta { get; init; }
        public string? StripeRefundId { get; init; }
        public DateTime? EnrolledAt { get; init; }
        public DateTime? DeadlineWas { get; init; }
    }
}
