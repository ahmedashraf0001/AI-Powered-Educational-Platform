$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\DependencyInjection.cs"
$content = Get-Content $filePath -Raw

$content = $content -replace "services.AddSingleton<IMaterialIndexingQueue, MaterialIndexingQueue>\(\);", "services.AddSingleton<IMaterialIndexingQueue, MaterialIndexingQueue>();`r`n            services.AddSingleton<ITagExtractionQueue, TagExtractionQueue>();"

Set-Content -Path $filePath -Value $content
