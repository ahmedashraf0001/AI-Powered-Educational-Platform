using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Materials;
using Pgvector;

namespace AIEduPlatform.Core.Interfaces.Repositories
{

    public interface IMaterialRepository : IGenericRepository<Material>
    {
        Task<Material?> GetMaterialByIdAsync(Guid materialId, bool includeChunks = false, CancellationToken ct = default);
        Task<Material?> GetMaterialByLectureIdAsync(Guid lectureId, bool includeChunks = false, CancellationToken ct = default);
        Task<List<Material>> GetMaterialByTitleAsync(string title, bool includeChunks = false, CancellationToken ct = default);
        Task<List<string>> GetMaterialFileUrlsByCourseIdAsync(Guid courseId, CancellationToken ct = default);
        Task<List<Material>> SearchMaterialsByTypeAsync(MaterialType type, bool includeChunks = false, CancellationToken ct = default);
        Task<List<MaterialSearchResult>> SearchMaterialsByEmbeddingAsync(Vector queryEmbedding, int topChunksPerMaterial = 3, CancellationToken ct = default);
        Task<MaterialSearchResult?> SearchChunksByMaterialAsync(Guid materialId, Vector queryEmbedding, int top = 5, CancellationToken ct = default);
        Task<MaterialSearchResult?> SearchByEmbeddingAndTextAsync(Vector queryEmbedding, string keyword, int top = 5, CancellationToken ct = default);
        Task AddRangeOfMaterialChunksAsync(IEnumerable<MaterialChunk> chunks, Guid materialId, CancellationToken ct = default);
        Task AddMaterialChunksAsync(MaterialChunk chunk, CancellationToken ct = default);
        Task<int> DeleteByIdAsync(Guid materialId, CancellationToken ct = default);
        Task<List<Material>> GetMaterialsToIndexAsync(Guid courseId, bool reindex, CancellationToken cancellationToken);
        Task<List<Material>> GetMaterialsForRetrievalAsync(
            Guid courseId,
            List<Guid>? lectureIds,
            List<Guid>? materialIds,
            List<MaterialType>? materialTypes,
            CancellationToken cancellationToken);
        Task<bool> HasUnindexedMaterialsAsync(Guid courseId, CancellationToken cancellationToken);
        Task<bool> HasUnindexedMaterialsInScopeAsync(
            Guid courseId,
            List<Guid>? lectureIds,
            List<Guid>? materialIds,
            CancellationToken cancellationToken);
        Task<int> GetMaterialsCountAsync(Guid courseId, CancellationToken cancellationToken);
    }
}
