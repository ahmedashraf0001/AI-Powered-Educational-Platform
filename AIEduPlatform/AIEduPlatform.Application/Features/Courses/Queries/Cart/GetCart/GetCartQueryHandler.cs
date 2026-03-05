using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Cart.GetCart
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, CartDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCartQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var cart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken);
            if (cart == null)
                return new CartDto();

            var items = cart.Items?.Select(i => new CartItemDto
            {
                CartItemId = i.Id,
                CourseId = i.CourseId,
                CourseTitle = i.Course?.Title ?? string.Empty,
                CourseThumbnailUrl = i.Course?.ThumbnailUrl,
                TeacherName = i.Course?.Teacher != null
                    ? $"{i.Course.Teacher.FirstName} {i.Course.Teacher.LastName}"
                    : string.Empty,
                OriginalPrice = i.Course?.Price ?? 0,
                PriceAtTimeOfAdding = i.PriceAtTimeOfAdding
            }).ToList() ?? [];

            return new CartDto
            {
                CartId = cart.Id,
                Items = items,
                ItemCount = items.Count,
                Subtotal = items.Sum(i => i.PriceAtTimeOfAdding),
                Currency = "usd"
            };
        }
    }
}
