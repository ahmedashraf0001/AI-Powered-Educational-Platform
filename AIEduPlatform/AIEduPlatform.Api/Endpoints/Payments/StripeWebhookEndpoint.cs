using AIEduPlatform.Application.Features.Courses.Commands.Payments.ConfirmPayment;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

namespace AIEduPlatform.Api.Endpoints.Payments;

public class StripeWebhookEndpoint : EndpointWithoutRequest
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StripeWebhookEndpoint> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public StripeWebhookEndpoint(IMediator mediator, IConfiguration configuration, ILogger<StripeWebhookEndpoint> logger, IUnitOfWork unitOfWork)
    {
        _mediator = mediator;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override void Configure()
    {
        Post("/api/payments/webhook");
        AllowAnonymous();
        Group<PaymentsGroup>();
        Summary(s =>
        {
            s.Summary = "Stripe webhook";
            s.Description = "Handles Stripe webhook events for payment confirmation. Do not call this endpoint directly.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var webhookSecret = _configuration["Stripe:WebhookSecret"];

        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                json,
                HttpContext.Request.Headers["Stripe-Signature"],
                webhookSecret);

            if (stripeEvent.Type == EventTypes.PaymentIntentSucceeded)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    _logger.LogInformation("Processing payment_intent.succeeded for {PaymentIntentId}", paymentIntent.Id);

                    await _mediator.Send(new ConfirmPaymentCommand
                    {
                        PaymentIntentId = paymentIntent.Id
                    }, ct);
                }
            }
            else if (stripeEvent.Type == EventTypes.PaymentIntentPaymentFailed)
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                if (paymentIntent != null)
                {
                    _logger.LogWarning("Payment failed for PaymentIntentId: {PaymentIntentId}", paymentIntent.Id);

                    // Update Order status to Failed
                    try
                    {
                        var order = await _unitOfWork.Orders.GetByStripePaymentIntentIdAsync(paymentIntent.Id, ct);
                        if (order != null && order.Status == OrderStatus.Pending)
                        {
                            order.Status = OrderStatus.Failed;
                            await _unitOfWork.Orders.UpdateAsync(order, ct);
                            await _unitOfWork.SaveChangesAsync(ct);
                            _logger.LogInformation("Order {OrderId} marked as Failed", order.Id);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating order status for failed payment {PaymentIntentId}", paymentIntent.Id);
                    }
                }
            }

            await SendOkAsync(ct);
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook signature verification failed");
            await SendAsync(null, 400, ct);
        }
        catch (Exception ex)
        {
            // Always return 200 to Stripe, even on internal errors
            _logger.LogError(ex, "Error processing Stripe webhook");
            await SendOkAsync(ct);
        }
    }
}
