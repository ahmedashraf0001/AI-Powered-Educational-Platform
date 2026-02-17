using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace AIEduPlatform.Application.Common.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<MaterialIndexingHub> _hubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<MaterialIndexingHub> hubContext,
            ILogger<NotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task NotifyIndexingCompletedAsync(
            Guid userId,
            RagIndexResponse response,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Sending indexing notification to teacher. UserId: {UserId}, CourseId: {CourseId}, Success: {Success}",
                    userId, response.CourseId, response.Success);

                // Send to specific user only
                await _hubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveIndexingNotification", response, cancellationToken);

                _logger.LogInformation(
                    "Indexing notification sent successfully. UserId: {UserId}, CourseId: {CourseId}",
                    userId, response.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send indexing notification. UserId: {UserId}, CourseId: {CourseId}",
                    userId, response.CourseId);
                // Don't throw - notification failure shouldn't break the indexing process
            }
        }
    }
}