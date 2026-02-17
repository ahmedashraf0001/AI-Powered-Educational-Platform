using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace AIEduPlatform.Application.SignalR
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // Extract user ID from JWT token claims
            // Adjust the claim type based on your JWT configuration
            // Common claim types: ClaimTypes.NameIdentifier, "sub", "userId", "nameid"

            var userId = connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? connection.User?.FindFirst("sub")?.Value
                      ?? connection.User?.FindFirst("userId")?.Value
                      ?? connection.User?.FindFirst("nameid")?.Value;

            return userId;
        }
    }
}
