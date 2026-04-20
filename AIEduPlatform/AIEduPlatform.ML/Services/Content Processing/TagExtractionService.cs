using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Prompts;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AIEduPlatform.ML.Services.Material_Processing
{
    public class TagExtractionService : ITagExtractionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IEmbeddingService _embeddingService;

        public TagExtractionService(
            IUnitOfWork unitOfWork,
            IOllamaServiceClient ollamaClient,
            IEmbeddingService embeddingService)
        {
            _unitOfWork = unitOfWork;
            _ollamaClient = ollamaClient;
            _embeddingService = embeddingService;

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
            await ReembedCourseTagsAsync(courseId, cancellationToken);

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
            await ReembedCourseTagsAsync(courseId, cancellationToken);

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

            var course = await _unitOfWork.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions { IncludeTags = true }, ct);
            if (course == null) return;

            var existingTagIds = course.CourseTags?
                .Select(ctag => ctag.TagId)
                .ToHashSet() ?? new HashSet<Guid>();

            if (replaceExisting)
            {
                // Remove with key-only join entities to avoid attaching detached Course navigation graphs.
                var linksToDelete = existingTagIds
                    .Select(tagId => new CourseTag
                    {
                        CourseId = courseId,
                        TagId = tagId
                    })
                    .ToList();

                if (linksToDelete.Any())
                {
                    _unitOfWork.Courses.RemoveRangeCourseTags(linksToDelete, ct);
                }

                var newCourseTags = tagEntities.Select(t => new CourseTag
                {
                    CourseId = courseId,
                    TagId = t.Id
                }).ToList();

                if (newCourseTags.Any())
                {
                    await _unitOfWork.Courses.AddRangeCourseTags(newCourseTags, ct);
                }
            }
            else
            {
                var newCourseTags = tagEntities
                    .Where(t => !existingTagIds.Contains(t.Id))
                    .Select(t => new CourseTag
                    {
                        CourseId = courseId,
                        TagId = t.Id
                    }).ToList();

                if (newCourseTags.Any())
                {
                    await _unitOfWork.Courses.AddRangeCourseTags(newCourseTags, ct);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }

        public async Task ReembedCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default)
        {
            var course = await _unitOfWork.Courses.GetCourseByIdAsync(
                courseId,
                new CourseIncludeOptions { IncludeTags = true },
                cancellationToken);

            if (course == null)
                return;

            var tagNames = (course.CourseTags ?? Enumerable.Empty<CourseTag>())
                .Select(ct => string.IsNullOrWhiteSpace(ct.Tag?.DisplayName) ? ct.Tag?.Name : ct.Tag?.DisplayName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var trackedCourse = await _unitOfWork.Courses.GetByIdAsync(courseId, cancellationToken);
            if (trackedCourse == null)
                return;

            var embeddingText = BuildCourseTagEmbeddingText(tagNames);

            if (string.IsNullOrWhiteSpace(embeddingText))
            {
                trackedCourse.TagEmbedding = null;
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                return;
            }

            var embeddingResponse = await _embeddingService.GetEmbeddingAsync(
                new EmbeddingRequest
                {
                    Text = embeddingText,
                    Normalize = true
                },
                cancellationToken);

            trackedCourse.TagEmbedding = embeddingResponse.Embedding?.Any() == true
                ? new Vector(embeddingResponse.Embedding.ToArray())
                : null;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static string BuildCourseTagEmbeddingText(IEnumerable<string> tagNames)
        {
            var orderedTags = tagNames
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var sb = new StringBuilder();

            foreach (var tag in orderedTags)
            {
                sb.Append(tag);
                sb.Append(' ');
            }

            return sb.ToString().Trim();
        }
        
        private static string ToTitleCase(string input)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo     
                .ToTitleCase(input);
        }

    }
}
