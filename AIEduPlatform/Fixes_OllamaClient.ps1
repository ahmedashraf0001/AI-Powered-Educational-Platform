$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.ML\Services\Models\OllamaServiceClient.cs"
$content = Get-Content $filePath -Raw

$stub = @"
        public async Task<AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto> ExtractCourseTagsAsync(
            AIEduPlatform.Core.DTOs.Tags.CourseTaggingDto course,
            CancellationToken ct = default)
        {
            // Placeholder: Not implemented natively in Ollama client yet by user
            return new AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto { CourseId = course.CourseId };
        }
"@

$content = $content -replace [regex]::Escape("    public class OllamaServiceClient : IOllamaServiceClient`r`n    {"), "    public class OllamaServiceClient : IOllamaServiceClient`r`n    {`r`n$stub"

Set-Content -Path $filePath -Value $content
