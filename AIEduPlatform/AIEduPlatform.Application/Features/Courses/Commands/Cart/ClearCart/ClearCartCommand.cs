using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.ClearCart
{
    public record ClearCartCommand : IRequest<Unit>;
}
