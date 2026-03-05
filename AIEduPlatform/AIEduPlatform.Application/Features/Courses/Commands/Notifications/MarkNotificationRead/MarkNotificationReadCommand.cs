using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.MarkNotificationRead
{
    public record MarkNotificationReadCommand : IRequest<Unit>
    {
        public Guid NotificationId { get; init; }
    }
}
