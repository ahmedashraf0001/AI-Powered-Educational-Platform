using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.MarkAllNotificationsRead
{
    public record MarkAllNotificationsReadCommand : IRequest<Unit>;
}
