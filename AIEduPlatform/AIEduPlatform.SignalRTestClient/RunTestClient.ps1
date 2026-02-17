# SignalR Test Client Helper Script
# This script helps you get a JWT token and test the SignalR connection

param(
    [string]$ApiBaseUrl = "https://localhost:7205",
    [switch]$UseExistingToken,
    [string]$Token
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   SignalR Test Client - Helper Script                 ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# If token is provided, skip to running the client
if ($UseExistingToken -and $Token) {
    Write-Host "✅ Using provided token" -ForegroundColor Green
    $jwtToken = $Token
} else {
    # Step 1: Check if API is running
    Write-Host "🔍 Checking if API is running at $ApiBaseUrl..." -ForegroundColor Yellow
    try {
        $healthCheck = Invoke-RestMethod -Uri "$ApiBaseUrl/health" -SkipCertificateCheck -TimeoutSec 5
        Write-Host "✅ API is running!" -ForegroundColor Green
        Write-Host ""
    } catch {
        Write-Host "❌ API is not responding at $ApiBaseUrl" -ForegroundColor Red
        Write-Host "   Please start the API first:" -ForegroundColor Yellow
        Write-Host "   cd ../AIEduPlatform.Api" -ForegroundColor Cyan
        Write-Host "   dotnet run" -ForegroundColor Cyan
        exit 1
    }

    # Step 2: Get credentials
    Write-Host "📋 Authentication" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────────" -ForegroundColor Gray

    $email = Read-Host "Enter email (default: teacher@test.com)"
    if ([string]::IsNullOrWhiteSpace($email)) {
        $email = "teacher@test.com"
    }

    $password = Read-Host "Enter password (default: Teacher123!)" -AsSecureString
    if ($password.Length -eq 0) {
        $passwordPlain = "Teacher123!"
    } else {
        $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
        $passwordPlain = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
    }

    Write-Host ""

    # Step 3: Try to login first
    Write-Host "🔐 Attempting to login..." -ForegroundColor Yellow
    $loginUrl = "$ApiBaseUrl/api/auth/login"
    $loginBody = @{
        email = $email
        password = $passwordPlain
    } | ConvertTo-Json

    try {
        $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json" -SkipCertificateCheck
        $jwtToken = $loginResponse.token
        Write-Host "✅ Login successful!" -ForegroundColor Green
    } catch {
        Write-Host "⚠️  Login failed. Attempting to register..." -ForegroundColor Yellow
        
        # Step 4: If login fails, try to register
        $registerUrl = "$ApiBaseUrl/api/auth/register"
        $registerBody = @{
            email = $email
            password = $passwordPlain
            role = "Teacher"
        } | ConvertTo-Json

        try {
            $registerResponse = Invoke-RestMethod -Uri $registerUrl -Method Post -Body $registerBody -ContentType "application/json" -SkipCertificateCheck
            Write-Host "✅ Registration successful!" -ForegroundColor Green
            
            # Login after registration
            Write-Host "🔐 Logging in..." -ForegroundColor Yellow
            $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $loginBody -ContentType "application/json" -SkipCertificateCheck
            $jwtToken = $loginResponse.token
            Write-Host "✅ Login successful!" -ForegroundColor Green
        } catch {
            Write-Host "❌ Registration and login failed: $($_.Exception.Message)" -ForegroundColor Red
            exit 1
        }
    }

    Write-Host ""
}

# Step 5: Save token for reference
Write-Host "📝 Your JWT Token (saved to token.txt):" -ForegroundColor Cyan
Write-Host "─────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host $jwtToken -ForegroundColor White
Write-Host ""
$jwtToken | Out-File -FilePath "token.txt" -Encoding UTF8

# Step 6: Parse the token to get user info (optional)
try {
    $tokenParts = $jwtToken.Split('.')
    $payloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($tokenParts[1] + "=="))
    $payload = $payloadJson | ConvertFrom-Json
    
    Write-Host "👤 Token Information:" -ForegroundColor Cyan
    Write-Host "   Email: $($payload.email)" -ForegroundColor White
    Write-Host "   Role: $($payload.role)" -ForegroundColor White
    Write-Host "   User ID: $($payload.nameid)" -ForegroundColor White
    Write-Host "   Expires: $(Get-Date -UnixTimeSeconds $payload.exp -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White
    Write-Host ""
} catch {
    # Token parsing failed, but continue anyway
}

# Step 7: Run the test client
Write-Host "🚀 Starting SignalR Test Client..." -ForegroundColor Green
Write-Host "─────────────────────────────────────────────────────" -ForegroundColor Gray
Write-Host ""

# Create a temporary file with the input
$inputFile = "client_input.txt"
@"
$ApiBaseUrl
$jwtToken
"@ | Out-File -FilePath $inputFile -Encoding UTF8

# Run the client with input from file
Get-Content $inputFile | dotnet run

# Cleanup
Remove-Item $inputFile -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "✅ Test client exited" -ForegroundColor Green
