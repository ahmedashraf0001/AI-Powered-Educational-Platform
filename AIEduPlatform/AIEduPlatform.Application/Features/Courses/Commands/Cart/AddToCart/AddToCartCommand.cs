using AIEduPlatform.Core.DTOs.Carts;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.AddToCart
{
    public record AddToCartCommand : IRequest<CartDto>
    {
        public Guid CourseId { get; init; }
    }
}
