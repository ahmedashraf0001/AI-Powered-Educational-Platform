using AIEduPlatform.Application.Features.Courses.Commands.Cart.ClearCart;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Cart;

public class ClearCartEndpoint : EndpointWithoutRequest<ApiResponse<object>>
{
    private readonly IMediator _mediator;

    public ClearCartEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/cart");
        Group<CartGroup>();
        Summary(s =>
        {
            s.Summary = "Clear cart";
            s.Description = "Removes all items from the current user's shopping cart.";
            s.Response<ApiResponse<object>>(200, "Cart cleared");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await _mediator.Send(new ClearCartCommand(), ct);
        await SendOkAsync(ApiResponse<object>.Ok(null!, "Cart cleared."), ct);
    }
}
