$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Core\Interfaces\Services\ITagExtractionService.cs"
$content = Get-Content $filePath -Raw

$updatedInterface = @"
    public interface ITagExtractionService
    {
        Task<CourseTagsResultDto> ExtractCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);

        Task<CourseTagsResultDto> ExtractCourseDeltaTagsAsync(
            Guid courseId,
            DateTime since,
            CancellationToken cancellationToken = default);
    }
"@

$content = $content -replace [regex]::Escape(@"
    public interface ITagExtractionService
    {
        Task<CourseTagsResultDto> ExtractCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);

    }
"@), $updatedInterface

Set-Content -Path $filePath -Value $content
