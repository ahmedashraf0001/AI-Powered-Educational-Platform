$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.ML\Services\Models\OllamaServiceClient.cs"
$content = Get-Content $filePath -Raw

$stub = @"
    public async Task<AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto> ExtractCourseTagsAsync(
        AIEduPlatform.Core.DTOs.Tags.CourseTaggingDto course,
        CancellationToken ct = default)
    {
        return new AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto { CourseId = course.CourseId };
    }
}
"@

$content = $content.TrimEnd()
$content = $content.Substring(0, $content.Length - 1) + $stub

Set-Content -Path $filePath -Value $content
