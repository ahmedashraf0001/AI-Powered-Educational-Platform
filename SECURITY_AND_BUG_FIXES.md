# Security & Bug Fixes Report

> Generated 2026-04-25 | AI-Powered Educational Platform
>
> Each section contains: the problem, the affected file(s), and the exact code to replace it with.
> No project files have been modified -- review each fix and apply as you see fit.

---

## Table of Contents

1. [CRITICAL: Hardcoded Secrets in Configuration](#1-critical-hardcoded-secrets-in-configuration)
2. [CRITICAL: CORS Allows All Origins with Credentials](#2-critical-cors-allows-all-origins-with-credentials)
3. [HIGH: SSRF in Vision Service URL Endpoint](#3-high-ssrf-in-vision-service-url-endpoint)
4. [HIGH: Path Traversal in FileService](#4-high-path-traversal-in-fileservice)
5. [HIGH: Path Traversal in Vision Service](#5-high-path-traversal-in-vision-service)
6. [HIGH: Path Traversal in Video Service](#6-high-path-traversal-in-video-service)
7. [HIGH: Missing Rate Limiting on Registration Endpoints](#7-high-missing-rate-limiting-on-registration-endpoints)
8. [HIGH: Missing Security Headers](#8-high-missing-security-headers)
9. [MEDIUM: Weak JWT Validation on Token Refresh](#9-medium-weak-jwt-validation-on-token-refresh)
10. [MEDIUM: Email Verification Token is a Plaintext GUID](#10-medium-email-verification-token-is-a-plaintext-guid)
11. [MEDIUM: File Upload Validates Extension Only, Not Content](#11-medium-file-upload-validates-extension-only-not-content)
12. [MEDIUM: Search Keyword Has No Length Limit](#12-medium-search-keyword-has-no-length-limit)
13. [MEDIUM: Silent Exception Swallowing in Exam Endpoint](#13-medium-silent-exception-swallowing-in-exam-endpoint)
14. [MEDIUM: Falsy-Value Bug in Video Analyzer Segment Parsing](#14-medium-falsy-value-bug-in-video-analyzer-segment-parsing)
15. [MEDIUM: Race Condition in Frontend Token Refresh](#15-medium-race-condition-in-frontend-token-refresh)
16. [MEDIUM: Temp File Cleanup Silently Fails](#16-medium-temp-file-cleanup-silently-fails)
17. [LOW: Wrong HTTP Status Code in Embedding Service](#17-low-wrong-http-status-code-in-embedding-service)

---

## 1. CRITICAL: Hardcoded Secrets in Configuration

**Files:**
- `AIEduPlatform/AIEduPlatform.Api/appsettings.json`
- `AIEduPlatform/AIEduPlatform.Api/appsettings.Development.json`
- `AIEduPlatform/docker-compose.yml`

**Problem:** Database passwords, JWT secrets, Stripe keys, Gmail credentials, and Azure API keys are all committed to the repository in plaintext. Anyone with read access to this repo can access your database, forge JWT tokens, process payments, and send emails as your application.

**What to do:**

### Step 1: Add `appsettings.Development.json` to `.gitignore`

```gitignore
# Add to .gitignore
appsettings.Development.json
appsettings.Production.json
```

### Step 2: Replace hardcoded values in `appsettings.json` with placeholders

```json
{
  "Stripe": {
    "SecretKey": "",
    "PublishableKey": "",
    "WebhookSecret": ""
  },
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "JWT": {
    "Secret": "",
    "ValidIssuer": "",
    "ValidAudience": ""
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "",
    "SenderName": "",
    "Username": "",
    "Password": "",
    "EnableSsl": true
  },
  "AIService": {
    "Groq": {
      "ApiKey": ""
    }
  }
}
```

### Step 3: Set secrets via environment variables or `dotnet user-secrets`

For development, use user-secrets:
```bash
dotnet user-secrets init --project AIEduPlatform.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=127.0.0.1;Port=5433;Database=AIEduPlatformDb;Username=postgres;Password=YOUR_PASSWORD"
dotnet user-secrets set "JWT:Secret" "YOUR_NEW_JWT_SECRET"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_..."
dotnet user-secrets set "EmailSettings:Password" "YOUR_APP_PASSWORD"
```

For production, use environment variables or Azure Key Vault:
```bash
export ConnectionStrings__DefaultConnection="Host=...;Password=..."
export JWT__Secret="..."
export Stripe__SecretKey="sk_live_..."
```

### Step 4: Rotate ALL exposed credentials immediately

Since these were committed to git history, the old values are permanently exposed. You must:
- Change the PostgreSQL password on all instances
- Generate a new JWT secret (this will invalidate all existing tokens -- users must re-login)
- Regenerate Stripe API keys in the Stripe dashboard
- Generate a new Gmail app password
- Rotate the Azure/Groq API key

---

## 2. CRITICAL: CORS Allows All Origins with Credentials

**File:** `AIEduPlatform/AIEduPlatform.Api/Extensions/CorsExtensions.cs`

**Problem:** `SetIsOriginAllowed(_ => true)` combined with `AllowCredentials()` means any website on the internet can make authenticated requests to your API. An attacker's site can silently call your endpoints using the victim's cookies/tokens.

**Current code (lines 5-19):**
```csharp
public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            // Temporary: allow requests from any origin.
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials(); // Required for SignalR
        });
    });
    return services;
}
```

**Fixed code:**
```csharp
public static IServiceCollection AddCorsPolicy(this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? ["http://localhost:5173"];

            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    });
    return services;
}
```

**Also update `Program.cs` registration (line 40):**
```csharp
// Before:
builder.Services.AddCorsPolicy();

// After:
builder.Services.AddCorsPolicy(builder.Configuration);
```

**Add to `appsettings.json`:**
```json
{
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://20.111.29.18"
    ]
  }
}
```

---

## 3. HIGH: SSRF in Vision Service URL Endpoint

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/vision_service/app/routes/vision.py`

**Problem:** The `/analyze/url` endpoint (line 240) and batch processing (line 397) fetch arbitrary user-supplied URLs with no validation. An attacker can use this to scan internal networks (`http://192.168.1.1/admin`), read local files (`file:///etc/passwd`), or hit cloud metadata endpoints (`http://169.254.169.254/`).

**Fix -- add a URL validation utility and apply it:**

Create a new file `vision_service/app/utils/url_validator.py`:
```python
import ipaddress
import socket
from urllib.parse import urlparse


BLOCKED_NETWORKS = [
    ipaddress.ip_network("10.0.0.0/8"),
    ipaddress.ip_network("172.16.0.0/12"),
    ipaddress.ip_network("192.168.0.0/16"),
    ipaddress.ip_network("127.0.0.0/8"),
    ipaddress.ip_network("169.254.0.0/16"),  # AWS metadata
    ipaddress.ip_network("0.0.0.0/8"),
    ipaddress.ip_network("::1/128"),
    ipaddress.ip_network("fc00::/7"),
    ipaddress.ip_network("fe80::/10"),
]

ALLOWED_SCHEMES = {"http", "https"}


def validate_url(url: str) -> None:
    """Validate that a URL is safe to fetch (no SSRF)."""
    parsed = urlparse(url)

    if parsed.scheme not in ALLOWED_SCHEMES:
        raise ValueError(f"URL scheme '{parsed.scheme}' is not allowed. Use http or https.")

    hostname = parsed.hostname
    if not hostname:
        raise ValueError("URL must contain a valid hostname.")

    try:
        resolved = socket.getaddrinfo(hostname, None)
    except socket.gaierror:
        raise ValueError(f"Cannot resolve hostname: {hostname}")

    for family, _, _, _, sockaddr in resolved:
        ip = ipaddress.ip_address(sockaddr[0])
        for network in BLOCKED_NETWORKS:
            if ip in network:
                raise ValueError(f"URL resolves to blocked internal address.")
```

**Update `/analyze/url` (lines 250-254):**
```python
# Before:
    try:
        async with httpx.AsyncClient(timeout=30.0) as client:
            response = await client.get(request.url)
            response.raise_for_status()
            image_bytes = response.content

# After:
    try:
        from app.utils.url_validator import validate_url
        validate_url(request.url)

        async with httpx.AsyncClient(timeout=30.0, follow_redirects=False) as client:
            response = await client.get(request.url)
            response.raise_for_status()
            image_bytes = response.content
```

**Update batch processing (lines 397-402):**
```python
# Before:
        elif item.url:
            # URL
            async with httpx.AsyncClient(timeout=30.0) as client:
                response = await client.get(item.url)
                response.raise_for_status()
                image_bytes = response.content

# After:
        elif item.url:
            from app.utils.url_validator import validate_url
            validate_url(item.url)
            async with httpx.AsyncClient(timeout=30.0, follow_redirects=False) as client:
                response = await client.get(item.url)
                response.raise_for_status()
                image_bytes = response.content
```

---

## 4. HIGH: Path Traversal in FileService

**File:** `AIEduPlatform/AIEduPlatform.Infrastructure/Services/FileService.cs`

**Problem:** `ResolvePhysicalPath` (line 158) does not verify that the resolved path stays within the uploads directory. An attacker who controls `fileUrl` can read or delete arbitrary files on the server (e.g., `../../appsettings.json`).

**Current code (lines 158-165):**
```csharp
public string ResolvePhysicalPath(string fileUrl)
{
    var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
    var pathWithinUploads = relativePath.StartsWith($"{UploadsFolder}{Path.DirectorySeparatorChar}")
        ? relativePath[($"{UploadsFolder}{Path.DirectorySeparatorChar}".Length)..]
        : relativePath;
    return Path.Combine(_uploadsPath, pathWithinUploads);
}
```

**Fixed code:**
```csharp
public string ResolvePhysicalPath(string fileUrl)
{
    var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
    var pathWithinUploads = relativePath.StartsWith($"{UploadsFolder}{Path.DirectorySeparatorChar}")
        ? relativePath[($"{UploadsFolder}{Path.DirectorySeparatorChar}".Length)..]
        : relativePath;

    var fullPath = Path.GetFullPath(Path.Combine(_uploadsPath, pathWithinUploads));
    var uploadsFullPath = Path.GetFullPath(_uploadsPath);

    if (!fullPath.StartsWith(uploadsFullPath + Path.DirectorySeparatorChar)
        && fullPath != uploadsFullPath)
    {
        throw new UnauthorizedAccessException(
            $"Access denied: path escapes the uploads directory.");
    }

    return fullPath;
}
```

---

## 5. HIGH: Path Traversal in Vision Service

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/vision_service/app/routes/vision.py`

**Problem:** The path traversal check at line 294 only looks for the literal string `..`. This misses encoded variants (`%2e%2e`), absolute paths, symlinks, and other bypass techniques.

**Current code (lines 291-302):**
```python
        path = Path(file_path)
        
        if ".." in str(path):
            raise ImageProcessingError("Invalid file path: directory traversal not allowed")
        
        if not path.exists():
            raise ImageProcessingError(f"File not found: {file_path}")
        
        if not path.is_file():
            raise ImageProcessingError(f"Path is not a file: {file_path}")
```

**Fixed code:**
```python
        ALLOWED_BASE_DIR = Path("/data/images").resolve()  # Configure this to your actual allowed dir

        path = Path(file_path).resolve()

        if not str(path).startswith(str(ALLOWED_BASE_DIR)):
            raise ImageProcessingError(
                "Invalid file path: access is restricted to the allowed directory"
            )

        if not path.exists():
            raise ImageProcessingError(f"File not found: {file_path}")

        if not path.is_file():
            raise ImageProcessingError(f"Path is not a file: {file_path}")
```

**Apply the same fix to the batch path handler (lines 384-395):**
```python
        elif item.path:
            ALLOWED_BASE_DIR = Path("/data/images").resolve()
            path = Path(item.path).resolve()
            if not str(path).startswith(str(ALLOWED_BASE_DIR)):
                raise ImageProcessingError(
                    "Invalid file path: access is restricted to the allowed directory"
                )
            if not path.exists():
                raise ImageProcessingError(f"File not found: {item.path}")
            if not path.is_file():
                raise ImageProcessingError(f"Path is not a file: {item.path}")
            validate_image_format(path.name)
            with open(path, "rb") as f:
                image_bytes = f.read()
```

---

## 6. HIGH: Path Traversal in Video Service

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/video_service/app/routes/video.py`

**Problem:** The `/analyze/path` endpoint (line 226) and `/context/{path:path}` endpoint (line 269) accept arbitrary file paths with no validation at all. An attacker can read any file on the filesystem that the process can access.

**Current code (lines 226-239):**
```python
@router.post("/analyze/path", response_model=VideoAnalysisResponse)
async def analyze_video_path(request: VideoPathRequest) -> VideoAnalysisResponse:
    analyzer = get_analyzer()
    
    if not os.path.exists(request.path):
        raise VideoProcessingError(f"Video file not found: {request.path}")
    
    validate_video_format(request.path)
```

**Fixed code:**
```python
import os

ALLOWED_VIDEO_DIR = os.environ.get("ALLOWED_VIDEO_DIR", "/data/videos")


def validate_path_safety(file_path: str) -> str:
    """Resolve path and ensure it stays within the allowed directory."""
    resolved = os.path.realpath(file_path)
    allowed = os.path.realpath(ALLOWED_VIDEO_DIR)
    if not resolved.startswith(allowed + os.sep) and resolved != allowed:
        raise VideoProcessingError("Access denied: path is outside the allowed directory")
    return resolved


@router.post("/analyze/path", response_model=VideoAnalysisResponse)
async def analyze_video_path(request: VideoPathRequest) -> VideoAnalysisResponse:
    analyzer = get_analyzer()

    safe_path = validate_path_safety(request.path)

    if not os.path.exists(safe_path):
        raise VideoProcessingError(f"Video file not found: {request.path}")

    validate_video_format(safe_path)
```

**Also fix `/context/{path:path}` (lines 269-284):**
```python
@router.get("/context/{path:path}")
async def get_video_context(
    path: str,
    frame_interval: float = Query(5.0, description="Seconds between frames"),
    max_frames: int = Query(30, description="Max frames to analyze"),
    include_timestamps: bool = Query(True, description="Include timestamps"),
    summary_format: bool = Query(False, description="Use summary format")
) -> dict:
    analyzer = get_analyzer()

    safe_path = validate_path_safety(path)

    if not os.path.exists(safe_path):
        raise VideoProcessingError(f"Video file not found: {path}")

    result = await analyzer.analyze_video_async(
        video_path=safe_path,
        frame_interval_seconds=frame_interval,
        max_frames=max_frames
    )
    # ... rest unchanged
```

---

## 7. HIGH: Missing Rate Limiting on Registration Endpoints

**Files:**
- `AIEduPlatform/AIEduPlatform.Api/Endpoints/Auth/RegisterStudentEndpoint.cs`
- `AIEduPlatform/AIEduPlatform.Api/Endpoints/Auth/RegisterTeacherEndpoint.cs`
- `AIEduPlatform/AIEduPlatform.Api/Endpoints/Auth/VerifyEmailEndpoint.cs`

**Problem:** The login endpoint has rate limiting, but registration and email verification do not. This allows mass account creation, email enumeration, and email bombing.

**Fix for `RegisterStudentEndpoint.cs` -- add rate limiting in `Configure()` (line 23-27):**
```csharp
public override void Configure()
{
    Post("/api/auth/register/student");
    AllowAnonymous();
    Group<AuthGroup>();
    Options(x => x.RequireRateLimiting(RateLimitingExtensions.LoginPolicy));
    // ... rest of Summary unchanged
}
```

**Fix for `RegisterTeacherEndpoint.cs` -- same change (line 24-28):**
```csharp
public override void Configure()
{
    Post("/api/auth/register/teacher");
    AllowAnonymous();
    Group<AuthGroup>();
    Options(x => x.RequireRateLimiting(RateLimitingExtensions.LoginPolicy));
    // ... rest of Summary unchanged
}
```

**Fix for `VerifyEmailEndpoint.cs` -- add rate limiting (line 22-26):**
```csharp
public override void Configure()
{
    Get("/api/auth/verify-email");
    AllowAnonymous();
    Group<AuthGroup>();
    Options(x => x.RequireRateLimiting(RateLimitingExtensions.LoginPolicy));
    // ... rest of Summary unchanged
}
```

---

## 8. HIGH: Missing Security Headers

**File:** `AIEduPlatform/AIEduPlatform.Api/Program.cs`

**Problem:** No HTTP security headers are configured. This leaves the application vulnerable to clickjacking, MIME-type sniffing attacks, and other browser-based exploits.

**Fix -- add security headers middleware. Insert after `app.UseExceptionHandler();` (line 72):**

```csharp
app.UseExceptionHandler();

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "0";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

    if (!context.Request.Path.StartsWithSegments("/uploads"))
    {
        context.Response.Headers["Content-Security-Policy"] =
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'; img-src 'self' data: https:; connect-src 'self' wss: ws:;";
    }

    await next();
});
```

If you deploy behind HTTPS (you should), also add HSTS:
```csharp
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
```

---

## 9. MEDIUM: Weak JWT Validation on Token Refresh

**File:** `AIEduPlatform/AIEduPlatform.Infrastructure/Services/JwtTokenGenerator.cs`

**Problem:** `GetPrincipalFromExpiredToken` (line 69) disables issuer and audience validation. This means a token forged with the correct signing key but a different issuer/audience would be accepted during refresh. While `ValidateLifetime = false` is necessary here (we're refreshing an expired token), the other validations should remain on.

**Current code (lines 69-101):**
```csharp
public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
{
    var jwtSettings = _configuration.GetSection("JWT");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");

    var tokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = false
    };
    // ...
}
```

**Fixed code:**
```csharp
public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
{
    var jwtSettings = _configuration.GetSection("JWT");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");
    var issuer = jwtSettings["ValidIssuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
    var audience = jwtSettings["ValidAudience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

    var tokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateLifetime = false
    };

    var tokenHandler = new JwtSecurityTokenHandler();

    try
    {
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
    catch
    {
        return null;
    }
}
```

---

## 10. MEDIUM: Email Verification Token is a Plaintext GUID

**Files:**
- `AIEduPlatform/AIEduPlatform.Application/Features/Auth/Commands/RegisterStudent/RegisterStudentCommandHandler.cs`
- `AIEduPlatform/AIEduPlatform.Application/Features/Auth/Commands/VerifyEmail/VerifyEmailCommandHandler.cs`

**Problem:** The verification token (line 44 of RegisterStudentCommandHandler) is `Guid.NewGuid().ToString("N")` -- a simple GUID. GUIDs are not cryptographically random (v4 GUIDs use a PRNG but have reduced entropy). The token is also stored in plaintext in the database, so a database breach exposes all pending verification tokens.

**Fix for `RegisterStudentCommandHandler.cs` -- replace GUID with crypto-random token (line 44):**
```csharp
// Before:
var verificationToken = Guid.NewGuid().ToString("N");

// After:
var verificationToken = Convert.ToHexString(
    System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
```

**Fix for `VerifyEmailCommandHandler.cs` -- use constant-time comparison (line 31):**
```csharp
// Before:
if (user.EmailVerificationToken != request.Token)
    throw new BadRequestException("Invalid verification token.");

// After:
if (user.EmailVerificationToken == null ||
    !CryptographicOperations.FixedTimeEquals(
        Encoding.UTF8.GetBytes(user.EmailVerificationToken),
        Encoding.UTF8.GetBytes(request.Token)))
{
    throw new BadRequestException("Invalid verification token.");
}
```

Add the required using at the top:
```csharp
using System.Security.Cryptography;
using System.Text;
```

---

## 11. MEDIUM: File Upload Validates Extension Only, Not Content

**File:** `AIEduPlatform/AIEduPlatform.Api/Endpoints/Materials/UploadMaterialEndpoint.cs`

**Problem:** File validation (line 62) only checks the filename extension. An attacker can rename a malicious executable to `malware.pdf` and upload it. The server then serves it back, potentially infecting other users who download it.

**Fix -- add MIME/magic-byte validation after the extension check (insert after line 62):**

```csharp
foreach (var file in req.Files)
{
    if (!FileExtensionConfiguration.IsSupported(file.FileName))
    {
        ThrowError($"File '{file.FileName}' has an unsupported format...");
        return;
    }

    // Validate that Content-Type matches the file extension
    var expectedContentTypes = FileExtensionConfiguration.GetExpectedContentTypes(file.FileName);
    if (expectedContentTypes != null && !expectedContentTypes.Contains(file.ContentType))
    {
        ThrowError($"File '{file.FileName}' content type '{file.ContentType}' does not match its extension.");
        return;
    }

    if (file.Length > MaxFileSize) { /* ... unchanged ... */ }
    if (file.Length == 0) { /* ... unchanged ... */ }
}
```

**Add a helper method to `FileExtensionConfiguration` (or wherever your extension config lives):**

```csharp
private static readonly Dictionary<string, HashSet<string>> ExtensionToContentTypes = new(StringComparer.OrdinalIgnoreCase)
{
    [".pdf"]  = ["application/pdf"],
    [".docx"] = ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
    [".pptx"] = ["application/vnd.openxmlformats-officedocument.presentationml.presentation"],
    [".mp4"]  = ["video/mp4"],
    [".mp3"]  = ["audio/mpeg"],
    [".png"]  = ["image/png"],
    [".jpg"]  = ["image/jpeg"],
    [".jpeg"] = ["image/jpeg"],
    [".gif"]  = ["image/gif"],
    [".webp"] = ["image/webp"],
};

public static HashSet<string>? GetExpectedContentTypes(string fileName)
{
    var ext = Path.GetExtension(fileName);
    return ext != null && ExtensionToContentTypes.TryGetValue(ext, out var types) ? types : null;
}
```

---

## 12. MEDIUM: Search Keyword Has No Length Limit

**File:** `AIEduPlatform/AIEduPlatform.Api/Endpoints/Courses/SearchCoursesEndpoint.cs`

**Problem:** The `Keyword` query parameter (line 12) has no max-length constraint. Very long strings (e.g., 100KB) can cause database performance degradation and potential ReDoS if any regex-based processing is involved downstream.

**Fix -- add validation to the request model:**
```csharp
public class SearchCoursesRequest
{
    [QueryParam]
    [MaxLength(200)]
    public string Keyword { get; set; } = string.Empty;
    [QueryParam]
    public int? Page { get; set; }
    [QueryParam]
    [Range(1, 100)]
    public int? PageSize { get; set; }
    [QueryParam]
    public Guid? CategoryId { get; set; }
}
```

Add using:
```csharp
using System.ComponentModel.DataAnnotations;
```

**Also clamp PageSize in the handler (line 48):**
```csharp
// Before:
PageSize = req.PageSize ?? 20

// After:
PageSize = Math.Clamp(req.PageSize ?? 20, 1, 100)
```

---

## 13. MEDIUM: Silent Exception Swallowing in Exam Endpoint

**File:** `AIEduPlatform/AIEduPlatform.Api/Endpoints/Exams/StartExamAttemptEndpoint.cs`

**Problem:** Line 81 has an empty `catch { /* ignore */ }` block when deserializing saved exam answers. If the JSON is corrupted or has an unexpected shape, this silently returns null answers to the student -- potentially losing their progress with no way to diagnose why.

**Current code (lines 77-82):**
```csharp
try
{
    savedAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(attempt.SavedAnswers);
}
catch { /* ignore */ }
```

**Fixed code:**
```csharp
try
{
    savedAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(attempt.SavedAnswers);
}
catch (JsonException ex)
{
    _logger.LogWarning(ex,
        "Failed to deserialize saved answers for attempt {AttemptId}. Data may be corrupted.",
        attempt.Id);
}
```

You'll need to inject `ILogger<StartExamAttemptEndpoint>` into the endpoint constructor:
```csharp
private readonly ILogger<StartExamAttemptEndpoint> _logger;

public StartExamAttemptEndpoint(
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService,
    ILogger<StartExamAttemptEndpoint> logger)
{
    _unitOfWork = unitOfWork;
    _currentUserService = currentUserService;
    _logger = logger;
}
```

---

## 14. MEDIUM: Falsy-Value Bug in Video Analyzer Segment Parsing

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/video_service/app/models/video_analyzer.py`

**Problem:** At line 266, the code uses `or` to pick between two dictionary keys:
```python
start = seg.get("start") or seg.get("start_time")
end = seg.get("end") or seg.get("end_time")
```
If `seg["start"]` is `0` (the first segment of a video starts at 0 seconds), Python treats `0` as falsy and skips to `seg.get("start_time")`. If `start_time` is also missing, `start` becomes `None` and the segment is silently dropped.

**Fixed code:**
```python
for seg in result.get("segments", []):
    start = seg.get("start")
    if start is None:
        start = seg.get("start_time")
    end = seg.get("end")
    if end is None:
        end = seg.get("end_time")
    text = seg.get("text", "").strip()
    if start is not None and end is not None and text:
        segments.append(TranscriptionSegment(
            text=text,
            start_time=float(start),
            end_time=float(end)
        ))
```

---

## 15. MEDIUM: Race Condition in Frontend Token Refresh

**File:** `AIEduPlatform/AIEduPlatform.UI/src/api/client.ts`

**Problem:** The global `isRefreshing` flag (line 26) and `failedQueue` (line 27) create a race condition. In a single-threaded JS runtime this is mostly safe, but the `await` at line 99 yields to the event loop, allowing another interceptor callback to read stale `isRefreshing` state. In rare cases, two refresh calls can fire simultaneously, causing one to fail and trigger an unnecessary logout.

**Fixed code -- replace lines 26-121 with a mutex-based approach:**
```typescript
let refreshPromise: Promise<string> | null = null;

client.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      const { accessToken, refreshToken, setTokens, logout } = useAuthStore.getState();
      if (!refreshToken) {
        logout();
        window.location.href = '/login';
        return Promise.reject(error);
      }

      originalRequest._retry = true;

      if (!refreshPromise) {
        refreshPromise = axios
          .post(`${API_URL}/auth/refresh-token`, { accessToken, refreshToken })
          .then(({ data }) => {
            const tokens = data.data;
            setTokens(tokens);
            return tokens.accessToken as string;
          })
          .catch((refreshError) => {
            logout();
            window.location.href = '/login';
            throw refreshError;
          })
          .finally(() => {
            refreshPromise = null;
          });
      }

      try {
        const newToken = await refreshPromise;
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return client(originalRequest);
      } catch (refreshError) {
        return Promise.reject(attachUserMessage(refreshError));
      }
    }

    return Promise.reject(attachUserMessage(error));
  }
);
```

This replaces both `isRefreshing` + `failedQueue` with a single shared promise. All concurrent 401 responses await the same refresh call -- no queue management needed, no race condition possible.

---

## 16. MEDIUM: Temp File Cleanup Silently Fails

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/video_service/app/routes/video.py`

**Problem:** The `cleanup_temp_file` function (lines 93-99) catches all exceptions with `pass`, silently leaking temp files on disk if cleanup fails. Over time this can fill the disk.

**Current code:**
```python
def cleanup_temp_file(path: str):
    """Remove temporary file."""
    try:
        if os.path.exists(path):
            os.remove(path)
    except Exception:
        pass
```

**Fixed code:**
```python
import logging

logger = logging.getLogger(__name__)

def cleanup_temp_file(path: str):
    """Remove temporary file."""
    try:
        if os.path.exists(path):
            os.remove(path)
    except OSError as e:
        logger.warning("Failed to clean up temp file %s: %s", path, e)
```

---

## 17. LOW: Wrong HTTP Status Code in Embedding Service

**File:** `AIEduPlatform/AIEduPlatform.PythonML/python-ai-services/embedding_service/app/routes/embeddings.py`

**Problem:** The catch-all exception handler at line 41 returns `500 Internal Server Error` for all non-`ValueError` exceptions. Some of these could be client errors (e.g., text too long, unsupported encoding) that should return `400`.

**Current code (lines 41-46):**
```python
    except Exception as e:
        logger.error(f"Error generating embedding: {str(e)}")
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to generate embedding"
        )
```

**Fixed code:**
```python
    except Exception as e:
        logger.error(f"Error generating embedding: {str(e)}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Internal error while generating embedding. Check server logs."
        )
```

The status code here is acceptable for truly unexpected errors, but add `exc_info=True` to get the full stack trace in logs for diagnosis. The real improvement is ensuring that known client errors (like exceeding model token limits) are caught as `ValueError` before this point, which they already are in the `encode_single` method. Verify that your `encode_single` method raises `ValueError` for all input validation issues.

---

## Summary

| # | Severity | Issue | File(s) |
|---|----------|-------|---------|
| 1 | CRITICAL | Hardcoded secrets in config | `appsettings.json`, `appsettings.Development.json` |
| 2 | CRITICAL | CORS allows all origins + credentials | `CorsExtensions.cs` |
| 3 | HIGH | SSRF via user-supplied URLs | `vision_service/routes/vision.py` |
| 4 | HIGH | Path traversal in file service | `FileService.cs` |
| 5 | HIGH | Path traversal in vision service | `vision_service/routes/vision.py` |
| 6 | HIGH | Path traversal in video service | `video_service/routes/video.py` |
| 7 | HIGH | No rate limiting on registration | `RegisterStudent/TeacherEndpoint.cs` |
| 8 | HIGH | No security headers | `Program.cs` |
| 9 | MEDIUM | Weak JWT validation on refresh | `JwtTokenGenerator.cs` |
| 10 | MEDIUM | Plaintext GUID verification token | `RegisterStudentCommandHandler.cs` |
| 11 | MEDIUM | Extension-only file validation | `UploadMaterialEndpoint.cs` |
| 12 | MEDIUM | Unbounded search keyword | `SearchCoursesEndpoint.cs` |
| 13 | MEDIUM | Silent exception swallowing | `StartExamAttemptEndpoint.cs` |
| 14 | MEDIUM | Falsy-value bug (0 == false) | `video_analyzer.py` |
| 15 | MEDIUM | Token refresh race condition | `client.ts` |
| 16 | MEDIUM | Silent temp file leak | `video_service/routes/video.py` |
| 17 | LOW | Wrong error status codes | `embedding_service/routes/embeddings.py` |
