$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Api\Program.cs"
$content = Get-Content $filePath -Raw

$content = $content -replace [regex]::Escape("builder.Services.AddApplication();"), "builder.Services.AddApplication(builder.Configuration);"

Set-Content -Path $filePath -Value $content
