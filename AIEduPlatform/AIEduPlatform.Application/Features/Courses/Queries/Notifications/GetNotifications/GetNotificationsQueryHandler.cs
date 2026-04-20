using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Notifications;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Notifications.GetNotifications
{
    public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, NotificationListDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<NotificationListDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var notifications = await _unitOfWork.Notifications.GetByUserIdAsync(
                userId, request.Page, request.PageSize, request.UnreadOnly, cancellationToken);

            var unreadCount = await _unitOfWork.Notifications.GetUnreadCountAsync(userId, cancellationToken);
            var totalCount = await _unitOfWork.Notifications.GetTotalCountAsync(userId, request.UnreadOnly, cancellationToken);

            var items = notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                RelatedEntityId = n.RelatedEntityId,
                RelatedEntityType = n.RelatedEntityType,
                Metadata = n.Metadata,
                CreatedAt = n.CreatedAt,
                ReadAt = n.ReadAt
            }).ToList();

            return new NotificationListDto
            {
                Items = items,
                UnreadCount = unreadCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
