using AIEduPlatform.Application.Features.Courses.Commands.Cart.AddToCart;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.DTOs.Common;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Cart;

public class AddToCartRequest
{
    public Guid CourseId { get; set; }
}

public class AddToCartEndpoint : Endpoint<AddToCartRequest, ApiResponse<CartDto>>
{
    private readonly IMediator _mediator;

    public AddToCartEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/cart/items");
        Group<CartGroup>();
        Summary(s =>
        {
            s.Summary = "Add course to cart";
            s.Description = "Adds a course to the current user's shopping cart. Captures the price at time of adding.";
            s.Response<ApiResponse<CartDto>>(200, "Course added to cart");
            s.Response(400, "Already enrolled, duplicate, or course not available");
            s.Response(401, "Not authenticated");
            s.Response(404, "Course not found");
        });
    }

    public override async Task HandleAsync(AddToCartRequest req, CancellationToken ct)
    {
        var cart = await _mediator.Send(new AddToCartCommand { CourseId = req.CourseId }, ct);
        await SendOkAsync(ApiResponse<CartDto>.Ok(cart, "Course added to cart."), ct);
    }
}
