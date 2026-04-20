$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.ML\Services\Content Processing\TagExtractionService.cs"
$content = Get-Content $filePath -Raw

$updatedBlock = @"
            var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions() { IncludeTags = true }, ct);
            if (course == null) return;

            var existingCourseTags = course.CourseTags != null ? course.CourseTags.ToList() : new List<CourseTag>();
"@

$search = @"
            var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions(), ct);
            if (course == null) return;

            var existingCourseTags = _unitOfWork.Tags.GetQueryableCourseTags().Where(ctag => ctag.CourseId == courseId).ToList();
"@

$content = $content -replace [regex]::Escape($search), $updatedBlock
Set-Content -Path $filePath -Value $content
