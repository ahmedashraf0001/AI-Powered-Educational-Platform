using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Notifications.GetUnreadCount
{
    public record GetUnreadCountQuery : IRequest<int>;
}
