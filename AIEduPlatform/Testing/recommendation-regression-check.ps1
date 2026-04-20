param(
    [string]$BaseUrl = "http://localhost:5069",
    [int]$Top = 10,
    [string]$AccessToken = $env:AIEDU_ACCESS_TOKEN,
    [string]$SnapshotFile = "",
    [switch]$UpdateSnapshot
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($SnapshotFile)) {
    $SnapshotFile = Join-Path $PSScriptRoot "recommendation.snapshot.json"
}

if ([string]::IsNullOrWhiteSpace($AccessToken)) {
    Write-Error "No access token provided. Pass -AccessToken or set AIEDU_ACCESS_TOKEN."
    exit 1
}

$uri = "$BaseUrl/api/courses/recommended?Top=$Top"

try {
    $response = Invoke-RestMethod -Method Get -Uri $uri -Headers @{ Authorization = "Bearer $AccessToken" }
}
catch {
    Write-Error "Recommendation endpoint call failed: $($_.Exception.Message)"
    exit 1
}

if (-not $response.success -or -not $response.data) {
    Write-Error "Endpoint returned empty or unsuccessful payload."
    exit 1
}

$actualIds = @($response.data | ForEach-Object { $_.courseId })
if ($actualIds.Count -eq 0) {
    Write-Error "No recommended course IDs returned."
    exit 1
}

if ($UpdateSnapshot -or -not (Test-Path $SnapshotFile)) {
    $newSnapshot = [ordered]@{
        top = $Top
        deterministicPrefixCount = [Math]::Max(1, [int][Math]::Floor($Top * 0.8))
        expectedCourseIds = $actualIds
        updatedAt = (Get-Date).ToString("o")
    }

    $newSnapshot | ConvertTo-Json -Depth 5 | Set-Content -Path $SnapshotFile -Encoding UTF8
    Write-Host "Recommendation snapshot updated at $SnapshotFile" -ForegroundColor Green
    exit 0
}

$snapshot = Get-Content -Path $SnapshotFile -Raw | ConvertFrom-Json
$expectedIds = @($snapshot.expectedCourseIds)

if ($expectedIds.Count -eq 0) {
    Write-Error "Snapshot has no expected IDs. Re-run with -UpdateSnapshot."
    exit 1
}

$prefixCount = [Math]::Min(
    [int]$snapshot.deterministicPrefixCount,
    [Math]::Min($expectedIds.Count, $actualIds.Count))
$prefixMismatches = @()

for ($i = 0; $i -lt $prefixCount; $i++) {
    if ($expectedIds[$i] -ne $actualIds[$i]) {
        $prefixMismatches += "Index $i expected '$($expectedIds[$i])' but got '$($actualIds[$i])'"
    }
}

$expectedSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$expectedIds)
$actualSet = [System.Collections.Generic.HashSet[string]]::new([string[]]$actualIds)

$intersectionCount = 0
foreach ($id in $actualSet) {
    if ($expectedSet.Contains($id)) {
        $intersectionCount++
    }
}

$overlapPct = if ($expectedSet.Count -gt 0) {
    [Math]::Round(($intersectionCount / [double]$expectedSet.Count) * 100, 2)
}
else {
    0
}

$minOverlapPct = 60

if ($prefixMismatches.Count -gt 0 -or $overlapPct -lt $minOverlapPct) {
    Write-Host "Recommendation regression check FAILED" -ForegroundColor Red
    Write-Host "Expected IDs: $($expectedIds -join ', ')" -ForegroundColor DarkGray
    Write-Host "Actual IDs:   $($actualIds -join ', ')" -ForegroundColor DarkGray
    Write-Host "Overlap: $overlapPct% (minimum $minOverlapPct%)" -ForegroundColor Yellow

    if ($prefixMismatches.Count -gt 0) {
        Write-Host "Deterministic prefix mismatches:" -ForegroundColor Yellow
        $prefixMismatches | ForEach-Object { Write-Host " - $_" -ForegroundColor Yellow }
    }

    exit 1
}

Write-Host "Recommendation regression check passed." -ForegroundColor Green
Write-Host "Overlap: $overlapPct%" -ForegroundColor Green
Write-Host "Deterministic prefix matched for first $prefixCount items." -ForegroundColor Green

