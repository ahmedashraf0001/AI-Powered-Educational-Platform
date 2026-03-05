using AIEduPlatform.Application.Features.Courses.Queries.Checkout.GetOrderStatus;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Payments;
using FastEndpoints;
using MediatR;

namespace AIEduPlatform.Api.Endpoints.Checkout;

public class GetOrderStatusRequest
{
    public Guid OrderId { get; set; }
}

public class GetOrderStatusEndpoint : Endpoint<GetOrderStatusRequest, ApiResponse<OrderStatusDto>>
{
    private readonly IMediator _mediator;

    public GetOrderStatusEndpoint(IMediator mediator) => _mediator = mediator;

    public override void Configure()
    {
        Get("/api/checkout/{OrderId}");
        Group<CheckoutGroup>();
        Summary(s =>
        {
            s.Summary = "Get order status";
            s.Description = "Returns the status of a checkout order, including payment status and enrolled courses.";
            s.Response<ApiResponse<OrderStatusDto>>(200, "Order status retrieved");
            s.Response(401, "Not authenticated");
            s.Response(404, "Order not found");
        });
    }

    public override async Task HandleAsync(GetOrderStatusRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetOrderStatusQuery { OrderId = req.OrderId }, ct);
        await SendOkAsync(ApiResponse<OrderStatusDto>.Ok(result), ct);
    }
}
