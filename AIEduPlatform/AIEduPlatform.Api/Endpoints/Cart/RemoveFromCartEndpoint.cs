using AIEduPlatform.Application.Features.Courses.Commands.Cart.RemoveFromCart;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Cart;

public class RemoveFromCartRequest
{
    public Guid CourseId { get; set; }
}

public class RemoveFromCartEndpoint : Endpoint<RemoveFromCartRequest, ApiResponse<CartDto>>
{
    private readonly IMediator _mediator;

    public RemoveFromCartEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Delete("/api/cart/items/{CourseId}");
        Group<CartGroup>();
        Summary(s =>
        {
            s.Summary = "Remove course from cart";
            s.Description = "Removes a specific course from the current user's shopping cart.";
            s.Response<ApiResponse<CartDto>>(200, "Course removed from cart");
            s.Response(401, "Not authenticated");
            s.Response(404, "Course not found in cart");
        });
    }

    public override async Task HandleAsync(RemoveFromCartRequest req, CancellationToken ct)
    {
        var cart = await _mediator.Send(new RemoveFromCartCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(ApiResponse<CartDto>.Ok(cart, "Course removed from cart."), ct);
    }
}
