namespace AIEduPlatform.Core.DTOs.Payments
{
    public class CreatePaymentIntentDto
    {
        public string ClientSecret { get; set; } = string.Empty;
        public string PaymentIntentId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "usd";
    }
}
