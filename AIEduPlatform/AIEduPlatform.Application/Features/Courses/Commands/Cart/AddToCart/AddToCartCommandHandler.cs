using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Carts;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.AddToCart
{
    public class AddToCartCommandHandler : IRequestHandler<AddToCartCommand, CartDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AddToCartCommandHandler> _logger;

        public AddToCartCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<AddToCartCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<CartDto> Handle(AddToCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to add items to the cart.");

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            if (!course.IsPublished)
                throw new BadRequestException("This course is not available for enrollment.");

            // Check if already enrolled
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(userId, request.CourseId, cancellationToken);
            if (isEnrolled)
                throw new BadRequestException("You are already enrolled in this course.");

            // Get or create active cart
            var cart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken);
            if (cart == null)
            {
                cart = new Core.Domain.Entities.Cart
                {
                    UserId = userId,
                    Status = CartStatus.Active,
                    Items = new List<CartItem>()
                };
                cart = await _unitOfWork.Carts.AddAsync(cart, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Check for duplicate
            var existingItem = cart.Items?.FirstOrDefault(i => i.CourseId == request.CourseId);
            if (existingItem != null)
                throw new BadRequestException("This course is already in your cart.");

            // Add item with price snapshot
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                CourseId = request.CourseId,
                PriceAtTimeOfAdding = course.Price,
                AddedAt = DateTime.UtcNow
            };

            await _unitOfWork.CartItems.AddAsync(cartItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Added course {CourseId} to cart {CartId} for user {UserId}", request.CourseId, cart.Id, userId);

            // Notify
            try
            {
                await _notificationService.NotifyCourseAddedToCartAsync(userId, course.Title, course.Id, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cart notification");
            }

            // Reload cart with items
            var updatedCart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken);
            return MapToDto(updatedCart!);
        }

        private static CartDto MapToDto(Core.Domain.Entities.Cart cart)
        {
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
