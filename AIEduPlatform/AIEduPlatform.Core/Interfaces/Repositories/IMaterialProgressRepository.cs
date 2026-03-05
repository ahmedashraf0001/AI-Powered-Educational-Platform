using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IMaterialProgressRepository : IGenericRepository<MaterialProgress>
    {
        Task<MaterialProgress?> GetProgressAsync(Guid studentId, Guid materialId, CancellationToken ct = default);
        Task<List<MaterialProgress>> GetProgressByCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
        Task<int> GetCompletedMaterialCountAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
        Task<MaterialProgress?> GetLastAccessedMaterialAsync(Guid studentId, Guid courseId, CancellationToken ct = default);
    }
}
