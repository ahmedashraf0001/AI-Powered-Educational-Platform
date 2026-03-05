using AIEduPlatform.Application.Features.Courses.Commands.Checkout.CreateCheckoutSession;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Payments;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Checkout;

public class CreateCheckoutEndpoint : EndpointWithoutRequest<ApiResponse<CheckoutResponseDto>>
{
    private readonly IMediator _mediator;

    public CreateCheckoutEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Post("/api/checkout");
        Group<CheckoutGroup>();
        Summary(s =>
        {
            s.Summary = "Create checkout session";
            s.Description = "Creates a checkout session from the current user's cart. Returns a Stripe client secret for payment, or completes the order immediately if all courses are free.";
            s.Response<ApiResponse<CheckoutResponseDto>>(200, "Checkout session created");
            s.Response(400, "Cart is empty or validation failed");
            s.Response(401, "Not authenticated");
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateCheckoutSessionCommand(), ct);
        await SendOkAsync(ApiResponse<CheckoutResponseDto>.Ok(result), ct);
    }
}
