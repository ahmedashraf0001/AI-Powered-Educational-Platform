$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Api\BackgroundServices\CourseTagUpdateBackgroundService.cs"
$content = Get-Content $filePath -Raw

$updatedWorker = @"
                    var coursesToUpdate = await unitOfWork.Courses.FindAsync(c => c.NeedsTagRebuild, stoppingToken);

                    foreach (var courseWithoutTags in coursesToUpdate)
                    {
                        var course = await unitOfWork.Courses.GetCourseByIdAsync(courseWithoutTags.Id, new AIEduPlatform.Core.Domain.Entities.CourseIncludeOptions { IncludeTags = true }, stoppingToken);
                        if (course == null) continue;

                        _logger.LogInformation("Processing tag updates for CourseId: {CourseId}. Pending changes: {Changes}", course.Id, course.PendingContentChanges);

                        try
                        {
                            var hasTags = course.CourseTags != null && course.CourseTags.Any();
"@

$searchWorker = @"
                    // Find courses needing tag rebuild
                    var coursesToUpdate = unitOfWork.Courses.GetQueryableCourses()
                        .Where(c => c.NeedsTagRebuild)
                        .ToList();

                    foreach (var course in coursesToUpdate)
                    {
                        _logger.LogInformation("Processing tag updates for CourseId: {CourseId}. Pending changes: {Changes}", course.Id, course.PendingContentChanges);

                        try
                        {
                            var hasTags = unitOfWork.Tags.GetQueryableCourseTags().Any(ct => ct.CourseId == course.Id);
"@

$content = $content -replace [regex]::Escape($searchWorker), $updatedWorker
Set-Content -Path $filePath -Value $content
