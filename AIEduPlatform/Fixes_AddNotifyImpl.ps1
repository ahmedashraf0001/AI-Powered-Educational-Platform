$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Common\Services\NotificationService.cs"
$content = Get-Content $filePath -Raw

$newMethod = @"
        public async Task NotifyIndexingCompletedAsync(
            Guid userId,
            RagIndexResponse response,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Sending indexing notification to teacher. UserId: {UserId}, CourseId: {CourseId}, Success: {Success}",
                    userId, response.CourseId, response.Success);

                await PersistNotificationAsync(userId, "IndexingCompleted",
                    response.Success ? "Indexing Complete" : "Indexing Failed",
                    response.Success
                        ? $"Material indexing completed for course {response.CourseId}"
                        : $"Material indexing failed for course {response.CourseId}",
                    response.CourseId, "Course", cancellationToken);

                await _teacherHubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveIndexingNotification", response, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send indexing completed notification. UserId={UserId}, CourseId={CourseId}", userId, response.CourseId);
            }
        }

        public async Task NotifyTagExtractionCompletedAsync(
            Guid userId,
            string courseTitle,
            bool success,
            string message,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Sending tag extraction notification to teacher. UserId: {UserId}, CourseTitle: {CourseTitle}, Success: {Success}",
                    userId, courseTitle, success);

                await PersistNotificationAsync(userId, "TagExtractionCompleted",
                    success ? "Tag Extraction Complete" : "Tag Extraction Failed",
                    success ? $"Tags were successfully extracted and generated for your course '{courseTitle}'." : $"Tag extraction failed for course '{courseTitle}'. Reason: {message}",
                    null, "Course", cancellationToken);

                await _teacherHubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveTagExtractionNotification", new { CourseTitle = courseTitle, Success = success, Message = message }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send tag extraction completed notification. UserId={UserId}", userId);
            }
        }
"@

$content = $content -replace "(?s)        public async Task NotifyIndexingCompletedAsync\(.*?\r?\n        }", $newMethod

Set-Content -Path $filePath -Value $content
