using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Recommedation;
using AIEduPlatform.Core.DTOs.Tags;
using AIEduPlatform.Core.Interfaces.Repositories;
using Pgvector;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetCourseByIdAsync(Guid courseId, CourseIncludeOptions options = default, CancellationToken ct = default);
        Task<List<Course>> GetCoursesByIdsAsync(IEnumerable<Guid> courseIds, CourseIncludeOptions options = default, CancellationToken ct = default);
        Task<List<Course>?> SearchCoursesByKeywordAsync(string keyword, CourseIncludeOptions options = null, CancellationToken ct = default);
        Task<(List<Course> Items, int TotalCount)> GetCoursesPagedAsync(
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default,
            Guid? categoryId = null);
        Task<(List<Course> Items, int TotalCount)> SearchCoursesPagedAsync(
            string keyword,
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default,
            Guid? categoryId = null);
        Task<(List<Course> Items, int TotalCount)> GetCoursesByInstructorPagedAsync(
            Guid instructorId,
            bool includeUnpublished,
            int page,
            int pageSize,
            CancellationToken ct = default);
        Task<int> DeleteByIdAsync(Guid courseId, CancellationToken ct = default);
        Task<bool> CourseExistsAsync(Guid courseId, CancellationToken cancellationToken);
        Task<bool> HasUnindexedMaterialsAsync(Guid courseId, CancellationToken cancellationToken);
        Task<List<Material>> GetMaterialsToIndexAsync(Guid courseId, bool reindex, CancellationToken cancellationToken);
        Task<int> GetMaterialsCountAsync(Guid courseId, CancellationToken cancellationToken);
        Task<List<CoursePopularityDto>> GetCoursePopularityAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default);
        Task<List<CourseQualityDto>> GetCourseQualityAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default);
        Task<List<CourseRecencyDto>> GetCourseRecencyAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default);
        Task<List<Guid>> GetCoursesBySimilarTagsAsync(
            IEnumerable<Guid> tagIds,
            int take,
            CancellationToken ct = default);
        Task<List<Guid>> GetTopPopularCoursesAsync(
            int take,
            CancellationToken ct = default);
        Task<List<Guid>> GetNewestCoursesAsync(
            int take,
            CancellationToken ct = default);
        Task<List<CourseTagsDto>> GetCourseTagsAsync(
            IEnumerable<Guid> courseIds,
            CancellationToken ct = default);
        Task AddRangeCourseTags(IEnumerable<CourseTag> courseTags, CancellationToken ct = default);
        void RemoveRangeCourseTags(IEnumerable<CourseTag> courseTags, CancellationToken ct = default);

        Task<List<Course>> GetSelectedCoursesAsync(IEnumerable<Guid> courseIds, CancellationToken ct = default, CourseIncludeOptions? options = null);
        Task<Dictionary<Guid, double>> GetSimilarityScoresAsync(
                    Vector userEmbedding,
                    IEnumerable<Guid> candidateCourseIds,
                    CancellationToken ct = default);
    }

}
