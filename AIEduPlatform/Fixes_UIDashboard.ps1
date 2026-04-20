$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.UI\src\pages\student\StudentDashboard.tsx"
$content = Get-Content $filePath -Raw

$oldVars = @"
  const currentStreak = 5;
  const weekDays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
  const activeDays = [true, true, false, true, true, true, false];
"@

$newVars = @"
  const currentStreak = dashboard?.streak?.currentStreak || 0;
  const weekDays = ['M', 'T', 'W', 'T', 'F', 'S', 'S'];
  const activeDays = dashboard?.streak?.activeDays || [false, false, false, false, false, false, false];
"@

$content = $content -replace [regex]::Escape($oldVars), $newVars

Set-Content -Path $filePath -Value $content
