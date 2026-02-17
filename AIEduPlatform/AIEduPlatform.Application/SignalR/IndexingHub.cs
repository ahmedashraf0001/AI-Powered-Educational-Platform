using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.SignalR
{
    [Authorize(Roles = "Teacher")] 
    public class MaterialIndexingHub : Hub
    {
        private readonly ILogger<MaterialIndexingHub> _logger;

        public MaterialIndexingHub(ILogger<MaterialIndexingHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("Teacher connected to MaterialIndexingHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            _logger.LogInformation("Teacher disconnected from MaterialIndexingHub. UserId: {UserId}, ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
    }
}