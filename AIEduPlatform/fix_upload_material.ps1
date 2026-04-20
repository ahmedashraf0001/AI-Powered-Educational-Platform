$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Courses\Commands\Materials\UploadMaterial\UploadMaterialCommandHandler.cs"
$content = Get-Content $filePath -Raw

$updatedCourseBlock = @"
                course.NeedsTagRebuild = true;
                course.PendingContentChanges += request.Files.Count;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _indexingQueue.EnqueueAsync(
"@

$content = $content -replace [regex]::Escape(@"
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _indexingQueue.EnqueueAsync(
"@), $updatedCourseBlock

Set-Content -Path $filePath -Value $content
