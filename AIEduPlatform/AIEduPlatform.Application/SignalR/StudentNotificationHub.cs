using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.SignalR
{
    [Authorize]
    public class StudentNotificationHub : Hub
    {
        private readonly ILogger<StudentNotificationHub> _logger;

        public StudentNotificationHub(ILogger<StudentNotificationHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Students join a course group to receive course-level notifications
        /// </summary>
        public async Task JoinCourseGroup(string courseId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"course-{courseId}");
            _logger.LogInformation(
                "User {UserId} joined course notification group {CourseId}",
                Context.UserIdentifier, courseId);
        }

        /// <summary>
        /// Students leave a course group
        /// </summary>
        public async Task LeaveCourseGroup(string courseId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"course-{courseId}");
            _logger.LogInformation(
                "User {UserId} left course notification group {CourseId}",
                Context.UserIdentifier, courseId);
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation(
                "User connected to StudentNotificationHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                Context.UserIdentifier, Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation(
                "User disconnected from StudentNotificationHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                Context.UserIdentifier, Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
