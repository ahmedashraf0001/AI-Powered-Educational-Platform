using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Payments.ConfirmPayment
{
    public record ConfirmPaymentCommand : IRequest<Unit>
    {
        public string PaymentIntentId { get; init; } = string.Empty;
    }
}
