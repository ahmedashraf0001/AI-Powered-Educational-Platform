$filePath = "F:\AI-Powered-Educational-Platform\AIEduPlatform\AIEduPlatform.ML\Services\Content Processing\TagExtractionService.cs"
$content = Get-Content $filePath -Raw

$updatedService = @"
    public class TagExtractionService : ITagExtractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOllamaServiceClient _ollamaClient;

        public TagExtractionService(IUnitOfWork unitOfWork, IOllamaServiceClient ollamaClient)
        {
            _unitOfWork = unitOfWork;
            _ollamaClient = ollamaClient;

        }
        private async Task<CourseTaggingDto> ExtractCourseMetadataAsync(Guid courseId, DateTime? since = null, CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions { IncludeLectures = true, IncludeMaterials = true}, cancellationToken);
            if (course == null)
                throw new KeyNotFoundException($"Course with id {courseId} not found");

            var lectures = course.Lectures.AsEnumerable();
            if (since.HasValue)
            {
                lectures = lectures.Where(l => l.CreatedAt >= since.Value || l.UpdatedAt >= since.Value || l.Materials.Any(m => m.CreatedAt >= since.Value || m.UpdatedAt >= since.Value));
            }

            return new CourseTaggingDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Description = course.Description,
                Lectures = lectures.Select(l => new LectureTaggingDto    
                {
                    Title = l.Title,
                    Description = l.Description,
                    Materials = l.Materials
                        .Where(m => !since.HasValue || (m.CreatedAt >= since.Value || m.UpdatedAt >= since.Value))
                        .Select(m => new MaterialTaggingDto  
                    {
                        Title = m.Title,
                        Summary = m.Summary,
                        Type = m.Type.ToString(),
                        DurationSeconds = m.DurationSeconds,
                        TotalPages = m.TotalPages,
                    }).ToList()
                }).ToList()
            };
        }
        
        public async Task<CourseTagsResultDto> ExtractCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            var courseDto = await ExtractCourseMetadataAsync(courseId, null, cancellationToken);
            var result = await _ollamaClient.ExtractCourseTagsAsync(courseDto, cancellationToken);

            result.Tags = await NormalizeTagsAsync(result.Tags, cancellationToken);
            result.Tags = await DeduplicateTagsAsync(result.Tags, cancellationToken);

            await SaveTagsAsync(courseId, result.Tags, true, cancellationToken);      

            return result;
        }

        public async Task<CourseTagsResultDto> ExtractCourseDeltaTagsAsync(
            Guid courseId,
            DateTime since,
            CancellationToken cancellationToken = default)
        {
            var courseDto = await ExtractCourseMetadataAsync(courseId, since, cancellationToken);
            if (!courseDto.Lectures.Any())
            {
                return new CourseTagsResultDto { CourseId = courseId, Tags = new List<string>() };
            }

            var result = await _ollamaClient.ExtractCourseTagsAsync(courseDto, cancellationToken);

            result.Tags = await NormalizeTagsAsync(result.Tags, cancellationToken);
            result.Tags = await DeduplicateTagsAsync(result.Tags, cancellationToken);

            await SaveTagsAsync(courseId, result.Tags, false, cancellationToken);      

            return result;
        }

        private Task<List<string>> DeduplicateTagsAsync(
            IEnumerable<string> tags,
            CancellationToken cancellationToken = default)
        {
            var deduplicated = tags
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Task.FromResult(deduplicated);
        }

        private Task<List<string>> NormalizeTagsAsync(
            IEnumerable<string> rawTags,
            CancellationToken cancellationToken = default)
        {
            var normalized = rawTags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLower())
                .Select(ToTitleCase)
                .ToList();

            return Task.FromResult(normalized);
        }
        
        private async Task SaveTagsAsync(Guid courseId, List<string> tags, bool replaceExisting, CancellationToken ct)
        {
            var tagEntities = await _unitOfWork.Tags.GetOrCreateAsync(tags, ct);
            
            var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions(), ct);
            if (course == null) return;
            
            var existingCourseTags = _unitOfWork.Tags.GetQueryableCourseTags().Where(ctag => ctag.CourseId == courseId).ToList();

            if (replaceExisting)
            {
                _unitOfWork.Tags.RemoveRangeCourseTags(existingCourseTags, ct);
                
                var newCourseTags = tagEntities.Select(t => new CourseTag
                {
                    CourseId = courseId,
                    TagId = t.Id
                }).ToList();
                await _unitOfWork.Tags.AddRangeCourseTags(newCourseTags, ct);
            }
            else
            {
                var existingTagIds = existingCourseTags.Select(ctag => ctag.TagId).ToHashSet();
                var newCourseTags = tagEntities
                    .Where(t => !existingTagIds.Contains(t.Id))
                    .Select(t => new CourseTag
                    {
                        CourseId = courseId,
                        TagId = t.Id
                    }).ToList();
                
                if (newCourseTags.Any())
                {
                    await _unitOfWork.Tags.AddRangeCourseTags(newCourseTags, ct);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
        
        private static string ToTitleCase(string input)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo     
                .ToTitleCase(input);
        }

    }
"@

$content = $content -replace '(?s)    public class TagExtractionService : ITagExtractionService.*?    }', $updatedService

Set-Content -Path $filePath -Value $content
