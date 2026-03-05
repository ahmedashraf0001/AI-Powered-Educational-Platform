using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface ISemanticSectionRepository : IGenericRepository<SemanticSection>
    {
        Task<List<SemanticSection>> GetByMaterialIdAsync(Guid materialId, CancellationToken ct = default);
        Task<SemanticSection?> GetSectionAtPositionAsync(Guid materialId, int position, CancellationToken ct = default);
    }
}
