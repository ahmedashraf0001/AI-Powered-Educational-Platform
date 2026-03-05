using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.RemoveFromCart
{
    public class RemoveFromCartCommandHandler : IRequestHandler<RemoveFromCartCommand, CartDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<RemoveFromCartCommandHandler> _logger;

        public RemoveFromCartCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<RemoveFromCartCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CartDto> Handle(RemoveFromCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var cart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken)
                ?? throw new NotFoundException("Cart not found.");

            var item = cart.Items?.FirstOrDefault(i => i.CourseId == request.CourseId)
                ?? throw new NotFoundException("Course not found in cart.");

            await _unitOfWork.CartItems.DeleteAsync(item, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Removed course {CourseId} from cart {CartId} for user {UserId}", request.CourseId, cart.Id, userId);

            // Reload
            var updatedCart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken);
            return MapToDto(updatedCart);
        }

        private static CartDto MapToDto(Core.Domain.Entities.Cart? cart)
        {
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
