$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.UI\src\types\index.ts"
$content = Get-Content $filePath -Raw

$newType = @"
export interface StudentDashboard {
  streak: { currentStreak: number; activeDays: boolean[] };
"@

$content = $content -replace "export interface StudentDashboard \{", $newType

Set-Content -Path $filePath -Value $content
