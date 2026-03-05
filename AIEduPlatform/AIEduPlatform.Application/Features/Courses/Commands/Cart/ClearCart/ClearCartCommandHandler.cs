using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Cart.ClearCart
{
    public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ClearCartCommandHandler> _logger;

        public ClearCartCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<ClearCartCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var cart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken);
            if (cart == null)
                return Unit.Value;

            if (cart.Items != null && cart.Items.Any())
            {
                await _unitOfWork.CartItems.DeleteRangeAsync(cart.Items, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Cleared cart {CartId} for user {UserId}", cart.Id, userId);

            try
            {
                await _notificationService.NotifyCartClearedAsync(userId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send cart cleared notification");
            }

            return Unit.Value;
        }
    }
}
