using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<Course?> GetCourseByIdAsync(Guid courseId, CourseIncludeOptions options = default, CancellationToken ct = default);
        Task<List<Course>?> SearchCoursesByKeywordAsync(string keyword, CourseIncludeOptions options = null, CancellationToken ct = default);
        Task<(List<Course> Items, int TotalCount)> GetCoursesPagedAsync(
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default);
        Task<(List<Course> Items, int TotalCount)> SearchCoursesPagedAsync(
            string keyword,
            bool onlyPublished,
            int page,
            int pageSize,
            CancellationToken ct = default);
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
    }

}
