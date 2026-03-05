using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Payments;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Checkout.GetOrderStatus
{
    public class GetOrderStatusQueryHandler : IRequestHandler<GetOrderStatusQuery, OrderStatusDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetOrderStatusQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<OrderStatusDto> Handle(GetOrderStatusQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
                ?? throw new NotFoundException("Order", request.OrderId);

            if (order.UserId != userId)
                throw new UnauthorizedException("You do not have access to this order.");

            var enrolledCourses = order.Items?.Select(oi => new EnrolledCourseInfoDto
            {
                CourseId = oi.CourseId,
                CourseTitle = oi.Course?.Title ?? string.Empty,
                Price = oi.Price
            }).ToList() ?? [];

            return new OrderStatusDto
            {
                OrderId = order.Id,
                Status = order.Status,
                PaidAt = order.PaidAt,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                EnrolledCourses = enrolledCourses
            };
        }
    }
}
