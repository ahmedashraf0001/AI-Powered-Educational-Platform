$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Api\Program.cs"
$content = Get-Content $filePath -Raw

$content = $content -replace "builder.Services.AddHostedService<MaterialIndexingBackgroundService>\(\);", "builder.Services.AddHostedService<MaterialIndexingBackgroundService>();`r`nbuilder.Services.AddHostedService<TagExtractionBackgroundService>();"

Set-Content -Path $filePath -Value $content
