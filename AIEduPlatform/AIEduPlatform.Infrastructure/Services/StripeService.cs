using AIEduPlatform.Core.DTOs.Payments;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AIEduPlatform.Infrastructure.Services
{
    public class StripeService : IStripeService
    {
        private readonly ILogger<StripeService> _logger;

        public StripeService(IConfiguration configuration, ILogger<StripeService> logger)
        {
            _logger = logger;
            StripeConfiguration.ApiKey = configuration["Stripe:SecretKey"];
        }

        public async Task<CreatePaymentIntentDto> CreatePaymentIntentAsync(
            long amount,
            string currency,
            Dictionary<string, string> metadata)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount,
                Currency = currency,
                Metadata = metadata,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            _logger.LogInformation(
                "Created Stripe PaymentIntent {PaymentIntentId} for amount {Amount} {Currency}",
                paymentIntent.Id, amount, currency);

            return new CreatePaymentIntentDto
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                Amount = (decimal)amount / 100,
                Currency = currency
            };
        }

        public async Task<string> CreateRefundAsync(string paymentIntentId, long amountInCents, string reason = "requested_by_customer")
        {
            var options = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = amountInCents,
                Reason = reason
            };

            var service = new RefundService();
            var refund = await service.CreateAsync(options);

            _logger.LogInformation(
                "Created Stripe Refund {RefundId} for PaymentIntent {PaymentIntentId}, Amount: {Amount}",
                refund.Id, paymentIntentId, amountInCents);

            return refund.Id;
        }

        public async Task CancelPaymentIntentAsync(string paymentIntentId)
        {
            var service = new PaymentIntentService();
            await service.CancelAsync(paymentIntentId);

            _logger.LogInformation("Cancelled Stripe PaymentIntent {PaymentIntentId}", paymentIntentId);
        }
    }
}
