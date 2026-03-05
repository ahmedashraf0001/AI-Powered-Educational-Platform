using AIEduPlatform.Core.DTOs.Payments;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Checkout.GetOrderStatus
{
    public record GetOrderStatusQuery : IRequest<OrderStatusDto>
    {
        public Guid OrderId { get; init; }
    }
}
