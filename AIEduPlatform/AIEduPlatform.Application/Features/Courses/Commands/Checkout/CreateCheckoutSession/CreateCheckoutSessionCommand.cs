using AIEduPlatform.Core.DTOs.Payments;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Checkout.CreateCheckoutSession
{
    public record CreateCheckoutSessionCommand : IRequest<CheckoutResponseDto>;
}
