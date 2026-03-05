using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.MarkNotificationRead
{
    public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public MarkNotificationReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var notification = await _unitOfWork.Notifications.GetByIdAsync(request.NotificationId, cancellationToken)
                ?? throw new NotFoundException("Notification", request.NotificationId);

            if (notification.UserId != userId)
                throw new UnauthorizedException("You do not have access to this notification.");

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _unitOfWork.Notifications.UpdateAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
