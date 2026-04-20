$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Core\Interfaces\Services\INotificationService.cs"
$content = Get-Content $filePath -Raw

$newMethod = @"
        Task NotifyIndexingCompletedAsync(Guid userId, RagIndexResponse response, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify a specific teacher about tag extraction completion
        /// </summary>
        Task NotifyTagExtractionCompletedAsync(Guid userId, string courseTitle, bool success, string message, CancellationToken cancellationToken = default);
"@

$content = $content -replace [regex]::Escape("        Task NotifyIndexingCompletedAsync(Guid userId, RagIndexResponse response, CancellationToken cancellationToken = default);"), $newMethod

Set-Content -Path $filePath -Value $content
