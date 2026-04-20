$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Api\Program.cs"
$content = Get-Content $filePath -Raw

$updatedProgram = @"
            builder.Services.AddHostedService<TagExtractionBackgroundService>();
            builder.Services.AddHostedService<CourseTagUpdateBackgroundService>();
"@

$searchProgram = @"
            builder.Services.AddHostedService<TagExtractionBackgroundService>();
"@

$content = $content -replace [regex]::Escape($searchProgram), $updatedProgram
Set-Content -Path $filePath -Value $content
