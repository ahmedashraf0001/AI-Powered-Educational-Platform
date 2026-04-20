$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Api\BackgroundServices\CourseTagUpdateBackgroundService.cs"
$content = Get-Content $filePath -Raw

$content = $content -replace "_unitOfWork", "unitOfWork"
Set-Content -Path $filePath -Value $content
