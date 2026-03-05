using AIEduPlatform.Application.Features.Courses.Queries.Cart.GetCart;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Cart;

public class GetCartEndpoint : EndpointWithoutRequest<ApiResponse<CartDto>>
{
    private readonly IMediator _mediator;

    public GetCartEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/cart");
        Group<CartGroup>();
        Summary(s =>
        {
            s.Summary = "Get current cart";
            s.Description = "Returns the current user's active shopping cart with all items and subtotal.";
            s.Response<ApiResponse<CartDto>>(200, "Cart retrieved successfully");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var cart = await _mediator.Send(new GetCartQuery(), ct);
        await SendOkAsync(ApiResponse<CartDto>.Ok(cart), ct);
    }
}
