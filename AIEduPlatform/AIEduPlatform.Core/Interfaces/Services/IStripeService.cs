using AIEduPlatform.Core.DTOs.Payments;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IStripeService
    {
        Task<CreatePaymentIntentDto> CreatePaymentIntentAsync(long amount, string currency, Dictionary<string, string> metadata);
        Task<string> CreateRefundAsync(string paymentIntentId, long amountInCents, string reason = "requested_by_customer");
        Task CancelPaymentIntentAsync(string paymentIntentId);
    }
}
