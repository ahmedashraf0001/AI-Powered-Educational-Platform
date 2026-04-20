$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Courses\Commands\Materials\DeleteMaterial\DeleteMaterialCommandHandler.cs"
$content = Get-Content $filePath -Raw

$updatedCourseBlock = @"
                    throw new InvalidOperationException($"Failed to delete material: {ragDeleteResult.Error}");
                }

                course.NeedsTagRebuild = true;
                course.PendingContentChanges += 1;
                course.HasContentDeletions = true;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully deleted material. MaterialId: {MaterialId}, Title: {Title}",
"@

$content = $content -replace [regex]::Escape(@"
                    throw new InvalidOperationException($"Failed to delete material: {ragDeleteResult.Error}");
                }

                _logger.LogInformation(
                    "Successfully deleted material. MaterialId: {MaterialId}, Title: {Title}",
"@), $updatedCourseBlock

Set-Content -Path $filePath -Value $content
