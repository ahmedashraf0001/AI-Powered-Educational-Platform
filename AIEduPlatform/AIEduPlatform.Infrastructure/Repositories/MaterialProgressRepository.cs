using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class MaterialProgressRepository : GenericRepository<MaterialProgress>, IMaterialProgressRepository
    {
        public MaterialProgressRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<MaterialProgress?> GetProgressAsync(Guid studentId, Guid materialId, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(mp => mp.StudentId == studentId && mp.MaterialId == materialId, ct);
        }

        public async Task<List<MaterialProgress>> GetProgressByCourseAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(mp => mp.Material)
                    .ThenInclude(m => m.Lecture)
                .Where(mp => mp.StudentId == studentId && mp.Material.Lecture.CourseId == courseId)
                .ToListAsync(ct);
        }

        public async Task<int> GetCompletedMaterialCountAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .CountAsync(mp =>
                    mp.StudentId == studentId
                    && mp.IsCompleted
                    && mp.Material.Lecture.CourseId == courseId, ct);
        }

        public async Task<MaterialProgress?> GetLastAccessedMaterialAsync(Guid studentId, Guid courseId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(mp => mp.Material)
                .Where(mp =>
                    mp.StudentId == studentId
                    && mp.Material.Lecture.CourseId == courseId
                    && !mp.IsCompleted)
                .OrderByDescending(mp => mp.UpdatedAt)
                .FirstOrDefaultAsync(ct);
        }
    }
}
