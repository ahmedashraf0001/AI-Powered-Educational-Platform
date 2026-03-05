using AIEduPlatform.Core.DTOs.Carts;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Cart.GetCart
{
    public record GetCartQuery : IRequest<CartDto>;
}
