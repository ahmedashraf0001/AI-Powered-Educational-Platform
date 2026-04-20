$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.UI\src\pages\student\StudentDashboard.tsx"
$content = Get-Content $filePath -Raw

$content = $content -replace "const \{ data: dashboard, isLoading: isDashboardLoading \} =", "const { data: dashboard } ="
$content = $content -replace 'variant="secondary"', 'variant="outline"'

Set-Content -Path $filePath -Value $content
