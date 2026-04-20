using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Notifications.DeleteAllNotifications;

public record DeleteAllNotificationsCommand : IRequest<Unit>;
