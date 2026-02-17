# SignalR Test Client

This is a standalone console application for testing the SignalR implementation in the AI Educational Platform.

## Features

- ✅ Connects to the MaterialIndexingHub
- ✅ Authenticates using JWT tokens
- ✅ Automatically reconnects if connection drops
- ✅ Displays real-time indexing notifications
- ✅ User-friendly console output with colors
- ✅ Handles SSL certificate validation (for development)

## Prerequisites

- .NET 8.0 SDK
- A running instance of AIEduPlatform.Api (default: https://localhost:7205)
- A valid JWT token with Teacher role

## Getting Started

### 1. Run the AI Educational Platform API

First, make sure the main API is running:

```bash
cd ../AIEduPlatform.Api
dotnet run
```

The API should be available at `https://localhost:7205` or `http://localhost:5069`.

### 2. Get a JWT Token

You need to authenticate as a Teacher to connect to the MaterialIndexingHub. Here are two ways to get a token:

#### Option A: Use the API directly

1. Register a new teacher account:
```powershell
$registerUrl = "https://localhost:7205/api/auth/register"
$body = @{
    email = "teacher@test.com"
    password = "Teacher123!"
    role = "Teacher"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $registerUrl -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck
```

2. Login and get the token:
```powershell
$loginUrl = "https://localhost:7205/api/auth/login"
$body = @{
    email = "teacher@test.com"
    password = "Teacher123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -SkipCertificateCheck
$token = $response.token
Write-Host "Your JWT Token: $token"
```

#### Option B: Use Postman or similar tools

1. POST to `/api/auth/register` with body:
```json
{
    "email": "teacher@test.com",
    "password": "Teacher123!",
    "role": "Teacher"
}
```

2. POST to `/api/auth/login` with body:
```json
{
    "email": "teacher@test.com",
    "password": "Teacher123!"
}
```

3. Copy the `token` from the response.

### 3. Run the Test Client

```bash
cd AIEduPlatform.SignalRTestClient
dotnet run
```

When prompted:
- Enter the API base URL (or press Enter to use default: `https://localhost:7205`)
- Paste your JWT token

### 4. Test the Notifications

Once connected, you can trigger notifications by:

1. **Using the test endpoint** (easiest way):
```powershell
# Get your userId from the token first or use a known userId
$testUrl = "https://localhost:7205/api/NotificationTest/test-notification"
$headers = @{
    "Authorization" = "Bearer YOUR_JWT_TOKEN"
    "Content-Type" = "application/json"
}
$body = @{
    userId = "YOUR_USER_ID_GUID"
    courseId = "SOME_COURSE_ID_GUID"
} | ConvertTo-Json

Invoke-RestMethod -Uri $testUrl -Method Post -Headers $headers -Body $body -SkipCertificateCheck
```

2. **Triggering actual material indexing**:
   - Upload course materials through the API
   - The background service will process them and send notifications

## Controls

While the client is running:
- Press **Q** to quit
- Press **S** to show connection status

## Notification Format

When a notification is received, you'll see something like:

```json
{
  "success": true,
  "courseId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "chunksIndexed": 150,
  "indexTimeMs": 5234,
  "embeddingTimeMs": 3421,
  "chunksFailed": 0,
  "failureRatio": 0
}
```

## Troubleshooting

### Connection Failed

If you see connection errors:

1. **Check if the API is running**: Visit `https://localhost:7205/health` in your browser
2. **Verify the JWT token**: Make sure it's valid and hasn't expired
3. **Check the role**: Only users with "Teacher" role can connect to MaterialIndexingHub
4. **CORS issues**: The API is configured to allow localhost origins

### SSL Certificate Errors

The client is configured to skip SSL certificate validation in development. If you still have issues, try using the HTTP endpoint instead:
- Change base URL to `http://localhost:5069`

### Token Expired

JWT tokens have an expiration time. If your token expires:
1. Login again to get a new token
2. Restart the test client with the new token

## Configuration

The hub URL is automatically constructed as:
```
{baseUrl}/hubs/material-indexing
```

The client uses automatic reconnection with the following retry pattern:
- Immediate retry (0 seconds)
- 2 seconds
- 5 seconds
- 10 seconds

## Development Notes

- The client logs SignalR internal events to the console
- SSL certificate validation is bypassed for development
- Automatic reconnection is enabled by default
- The connection uses the `AccessTokenProvider` for authentication

## Related Files

- **Hub**: `AIEduPlatform.Application/SignalR/IndexingHub.cs`
- **Notification Service**: `AIEduPlatform.Application/Common/Services/NotificationService.cs`
- **Background Service**: `AIEduPlatform.Api/BackgroundServices/MaterialIndexingBackgroundService.cs`
- **Test Controller**: `AIEduPlatform.Api/Endpoints/NotificationController.cs`
