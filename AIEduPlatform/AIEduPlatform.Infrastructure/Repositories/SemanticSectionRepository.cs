using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class SemanticSectionRepository : GenericRepository<SemanticSection>, ISemanticSectionRepository
    {
        public SemanticSectionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<SemanticSection>> GetByMaterialIdAsync(Guid materialId, CancellationToken ct = default)
        {
            return await _dbSet
                .Where(s => s.MaterialId == materialId)
                .OrderBy(s => s.OrderIndex)
                .ToListAsync(ct);
        }

        public async Task<SemanticSection?> GetSectionAtPositionAsync(Guid materialId, int position, CancellationToken ct = default)
        {
            var material = await _context.Materials.FindAsync(new object[] { materialId }, ct);
            if (material == null) return null;

            if (material.Type == MaterialType.Video || material.Type == MaterialType.Audio)
            {
                return await _dbSet
                    .Where(s => s.MaterialId == materialId
                        && s.StartSeconds.HasValue && s.EndSeconds.HasValue
                        && s.StartSeconds.Value <= position && s.EndSeconds.Value >= position)
                    .FirstOrDefaultAsync(ct);
            }
            else
            {
                return await _dbSet
                    .Where(s => s.MaterialId == materialId
                        && s.StartPage.HasValue && s.EndPage.HasValue
                        && s.StartPage.Value <= position && s.EndPage.Value >= position)
                    .FirstOrDefaultAsync(ct);
            }
        }
    }
}
