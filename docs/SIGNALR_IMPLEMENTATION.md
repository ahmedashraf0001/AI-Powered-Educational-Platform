# SignalR Implementation - Changes and Testing Guide

## Summary of Changes

### 1. Fixed Hub Namespace Issue ✅
**Problem**: The `MaterialIndexingHub` was in the wrong namespace (`AIEduPlatform.Api.Hubs` instead of `AIEduPlatform.Application.SignalR`).

**Files Changed**:
- `AIEduPlatform.Application/SignalR/IndexingHub.cs` - Fixed namespace
- `AIEduPlatform.Api/Program.cs` - Updated using directive
- `AIEduPlatform.Application/Common/Services/NotificationService.cs` - Updated using directive

### 2. Fixed CORS Configuration ✅
**Problem**: The original CORS policy used `AllowAnyOrigin()` which is incompatible with SignalR's credential requirement.

**Solution**: Updated CORS to allow specific localhost origins with credentials support.

**File Changed**:
- `AIEduPlatform.Api/Extensions/CorsExtensions.cs`

**New Configuration**:
```csharp
policy.WithOrigins(
    "http://localhost:3000",
    "https://localhost:3000",
    "http://localhost:5069",
    "https://localhost:7205")
.AllowAnyMethod()
.AllowAnyHeader()
.AllowCredentials(); // Required for SignalR
```

### 3. Created Standalone Test Client ✅
**New Project**: `AIEduPlatform.SignalRTestClient`

A fully functional console application for testing SignalR connections with:
- JWT authentication
- Automatic reconnection
- SSL certificate handling (for development)
- Colorized console output
- Real-time notification display

**Files Created**:
- `AIEduPlatform.SignalRTestClient/AIEduPlatform.SignalRTestClient.csproj`
- `AIEduPlatform.SignalRTestClient/Program.cs`
- `AIEduPlatform.SignalRTestClient/README.md`
- `AIEduPlatform.SignalRTestClient/RunTestClient.ps1` (Helper script)
- `AIEduPlatform.SignalRTestClient/run.bat` (Quick start batch file)
- `AIEduPlatform.SignalRTestClient/.gitignore`

## Current SignalR Architecture

### Hub: MaterialIndexingHub
- **Location**: `AIEduPlatform.Application/SignalR/IndexingHub.cs`
- **Endpoint**: `/hubs/material-indexing`
- **Authorization**: Requires "Teacher" role
- **Event**: `ReceiveIndexingNotification`

### User ID Provider
- **Location**: `AIEduPlatform.Application/SignalR/CustomUserIdProvider.cs`
- **Purpose**: Extracts user ID from JWT token claims
- **Supported Claims**: `NameIdentifier`, `sub`, `userId`, `nameid`

### Notification Service
- **Location**: `AIEduPlatform.Application/Common/Services/NotificationService.cs`
- **Interface**: `INotificationService`
- **Method**: `NotifyIndexingCompletedAsync(Guid userId, RagIndexResponse response, CancellationToken)`
- **Behavior**: Sends notifications to specific users via SignalR

### Background Service
- **Location**: `AIEduPlatform.Api/BackgroundServices/MaterialIndexingBackgroundService.cs`
- **Purpose**: Processes material indexing requests and sends notifications through SignalR
- **Uses**: `IMaterialIndexingQueue`, `IRAGService`, `INotificationService`

## Configuration

### API Endpoints
- **Development HTTPS**: `https://localhost:7205`
- **Development HTTP**: `http://localhost:5069`
- **SignalR Hub**: `{baseUrl}/hubs/material-indexing`
- **Health Check**: `{baseUrl}/health`

### Authentication
- JWT tokens are required
- Teacher role is required for MaterialIndexingHub
- Token is passed via `AccessTokenProvider` in SignalR client

### CORS
Currently configured for localhost development on ports:
- 3000 (typical frontend)
- 5069 (API HTTP)
- 7205 (API HTTPS)

## How to Test

### Quick Start (Easiest)
```powershell
# Terminal 1: Start the API
cd AIEduPlatform.Api
dotnet run

# Terminal 2: Run the test client helper
cd ../AIEduPlatform.SignalRTestClient
.\RunTestClient.ps1
```

The helper script will:
1. Check if the API is running
2. Prompt for credentials (or use defaults)
3. Register/Login to get a JWT token
4. Automatically start the test client
5. Save the token to `token.txt` for reference

### Manual Testing

#### Step 1: Start the API
```bash
cd AIEduPlatform.Api
dotnet run
```

Wait for the message: "Now listening on: https://localhost:7205"

#### Step 2: Get a JWT Token

**Option A - Using PowerShell**:
```powershell
# Register
$registerUrl = "https://localhost:7205/api/auth/register"
$body = @{
    email = "teacher@test.com"
    password = "Teacher123!"
    role = "Teacher"
} | ConvertTo-Json

Invoke-RestMethod -Uri $registerUrl -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck

# Login
$loginUrl = "https://localhost:7205/api/auth/login"
$body = @{
    email = "teacher@test.com"
    password = "Teacher123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck
$token = $response.token
Write-Host $token
```

**Option B - Using curl**:
```bash
# Register
curl -k -X POST https://localhost:7205/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"teacher@test.com","password":"Teacher123!","role":"Teacher"}'

# Login
curl -k -X POST https://localhost:7205/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"teacher@test.com","password":"Teacher123!"}'
```

#### Step 3: Run the Test Client
```bash
cd AIEduPlatform.SignalRTestClient
dotnet run
```

Enter the API URL and paste your JWT token when prompted.

#### Step 4: Trigger a Notification

**Using the Test Controller** (requires the token):
```powershell
$testUrl = "https://localhost:7205/api/NotificationTest/test-notification"
$headers = @{
    "Authorization" = "Bearer YOUR_JWT_TOKEN"
    "Content-Type" = "application/json"
}
$body = @{
    userId = "YOUR_USER_ID_FROM_TOKEN"
    courseId = "00000000-0000-0000-0000-000000000001"
} | ConvertTo-Json

Invoke-RestMethod -Uri $testUrl -Method Post -Headers $headers -Body $body -SkipCertificateCheck
```

**Real Material Indexing**:
Upload course materials through the API, and the background service will automatically send notifications when indexing completes.

## Notification Format

When material indexing completes, clients receive a notification with this structure:

```json
{
  "success": true,
  "courseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "chunksIndexed": 150,
  "indexTimeMs": 5234,
  "embeddingTimeMs": 3421,
  "chunksFailed": 0,
  "failureRatio": 0,
  "error": null
}
```

## Troubleshooting

### Issue: "Connection failed: Response status code does not indicate success: 401 (Unauthorized)"
**Cause**: Invalid or expired JWT token
**Solution**: Get a new token and try again

### Issue: "Connection failed: Response status code does not indicate success: 403 (Forbidden)"
**Cause**: User doesn't have "Teacher" role
**Solution**: Make sure to register with `"role": "Teacher"`

### Issue: "API is not responding"
**Cause**: API is not running or running on different port
**Solution**: Start the API and verify the port in launchSettings.json

### Issue: No notifications received
**Possible Causes**:
1. Wrong user ID - notifications are sent to specific users
2. Connection lost - check client console for disconnection messages
3. Background service not running - check API logs

**Solution**: 
- Verify the user ID in the token matches the notification target
- Check API logs for notification send attempts
- Ensure the test endpoint is being called with correct parameters

### Issue: CORS errors
**Cause**: Frontend running on a port not listed in CORS configuration
**Solution**: Add the port to `CorsExtensions.cs` in the `WithOrigins()` array

## Development Notes

### For Frontend Developers

To connect from a JavaScript/TypeScript frontend:

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7205/hubs/material-indexing", {
    accessTokenFactory: () => getYourJwtToken()
  })
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();

connection.on("ReceiveIndexingNotification", (notification) => {
  console.log("Received notification:", notification);
  // Handle the notification in your UI
});

await connection.start();
```

### Adding More Hubs

To add additional SignalR hubs:

1. Create a new hub class in `AIEduPlatform.Application/SignalR/`
2. Register it in `Program.cs`: `app.MapHub<YourHub>("/hubs/your-hub");`
3. Update CORS if needed
4. Create notification methods in an interface and service

### Security Considerations

- JWT tokens expire after a configured time
- Always use HTTPS in production
- Update CORS origins for production deployment
- Consider rate limiting for hub connections
- Log all SignalR connection/disconnection events

## Next Steps

1. **Production Readiness**:
   - Update CORS origins for your production domain
   - Enable proper SSL certificates
   - Configure authentication expiration appropriately
   - Add connection limits and rate limiting

2. **Feature Enhancements**:
   - Add more notification types (quiz results, grade updates, etc.)
   - Implement group notifications (all students in a course)
   - Add notification history/persistence
   - Create a web-based test client

3. **Monitoring**:
   - Add application insights or logging for SignalR events
   - Track connection count and duration
   - Monitor notification delivery rates
   - Set up alerts for connection failures

## References

- [ASP.NET Core SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [SignalR JavaScript Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [SignalR .NET Client](https://docs.microsoft.com/en-us/aspnet/core/signalr/dotnet-client)
