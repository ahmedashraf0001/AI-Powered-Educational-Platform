$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.Application\Features\Courses\Commands\Lectures\AddLecture\AddLectureCommandHandler.cs"
$content = Get-Content $filePath -Raw

$updatedCourseBlock = @"
                var lecture = new Lecture
                {
                    CourseId = request.CourseId,
                    Title = request.Title,
                    Description = request.Description,
                    OrderIndex = request.OrderIndex,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                course.NeedsTagRebuild = true;
                course.PendingContentChanges += 1;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);

                var createdLecture = await _unitOfWork.Lectures.AddAsync(lecture, cancellationToken);
"@

$content = $content -replace [regex]::Escape(@"
                var lecture = new Lecture
                {
                    CourseId = request.CourseId,
                    Title = request.Title,
                    Description = request.Description,
                    OrderIndex = request.OrderIndex,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var createdLecture = await _unitOfWork.Lectures.AddAsync(lecture, cancellationToken);
"@), $updatedCourseBlock

Set-Content -Path $filePath -Value $content
