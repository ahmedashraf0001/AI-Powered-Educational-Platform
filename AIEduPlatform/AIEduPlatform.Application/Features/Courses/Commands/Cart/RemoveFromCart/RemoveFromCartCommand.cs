using AIEduPlatform.Core.DTOs.Carts;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.RemoveFromCart
{
    public record RemoveFromCartCommand : IRequest<CartDto>
    {
        public Guid CourseId { get; init; }
    }
}
