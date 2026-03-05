using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.DeleteNotification
{
    public record DeleteNotificationCommand : IRequest<Unit>
    {
        public Guid NotificationId { get; init; }
    }
}
