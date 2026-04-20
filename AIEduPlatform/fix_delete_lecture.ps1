$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Courses\Commands\Lectures\DeleteLecture\DeleteLectureCommandHandler.cs"
$content = Get-Content $filePath -Raw

$updatedCourseBlock = @"
                    throw new InvalidOperationException($"Failed to delete lecture: {ragDeleteResult.Error}");
                }

                course.NeedsTagRebuild = true;
                course.PendingContentChanges += 1;
                course.HasContentDeletions = true;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
"@

$content = $content -replace [regex]::Escape(@"
                    throw new InvalidOperationException($"Failed to delete lecture: {ragDeleteResult.Error}");
                }

                _logger.LogInformation(
"@), $updatedCourseBlock

Set-Content -Path $filePath -Value $content
