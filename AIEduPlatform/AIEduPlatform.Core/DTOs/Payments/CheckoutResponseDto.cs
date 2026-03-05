namespace AIEduPlatform.Core.DTOs.Payments
{
    public record CheckoutResponseDto
    {
        public Guid OrderId { get; init; }
        public string? ClientSecret { get; init; }
        public string? PaymentIntentId { get; init; }
        public string? PublishableKey { get; init; }
        public bool RequiresPayment { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = "usd";
        public List<CheckoutItemDto> Items { get; init; } = [];
    }

    public record CheckoutItemDto
    {
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public decimal Price { get; init; }
    }
}
