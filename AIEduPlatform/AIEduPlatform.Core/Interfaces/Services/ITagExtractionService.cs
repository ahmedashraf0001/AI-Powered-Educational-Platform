using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Tags;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface ITagExtractionService
    {
        Task<CourseTagsResultDto> ExtractCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);

        Task<CourseTagsResultDto> ExtractCourseDeltaTagsAsync(
            Guid courseId,
            DateTime since,
            CancellationToken cancellationToken = default);

        Task ReembedCourseTagsAsync(
            Guid courseId,
            CancellationToken cancellationToken = default);
    }
}
