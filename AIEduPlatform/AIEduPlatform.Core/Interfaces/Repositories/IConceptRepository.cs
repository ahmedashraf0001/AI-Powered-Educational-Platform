using AIEduPlatform.Core.Domain.Entities;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    public interface IConceptRepository
    {
        // Write
        Task AddConceptsAsync(IEnumerable<Concept> concepts, CancellationToken ct = default);
        Task AddRelationsAsync(IEnumerable<ConceptRelation> relations, CancellationToken ct = default);
        Task AddChunkMapsAsync(IEnumerable<ConceptChunkMap> maps, CancellationToken ct = default);
        Task DeleteByCourseIdAsync(Guid courseId, CancellationToken ct = default);

        // Lookup
        Task<List<Concept>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);
        Task<List<Concept>> FindByNormalizedNamesAsync(Guid courseId, IEnumerable<string> normalizedNames, CancellationToken ct = default);
        Task<List<Concept>> SearchByEmbeddingAsync(Guid courseId, Vector queryEmbedding, int topK = 10, CancellationToken ct = default);

        // Graph traversal
        Task<List<Guid>> GetNeighbourConceptIdsAsync(IEnumerable<Guid> conceptIds, int depth = 2, CancellationToken ct = default);

        // Chunk resolution
        Task<List<Guid>> GetChunkIdsByConceptIdsAsync(IEnumerable<Guid> conceptIds, CancellationToken ct = default);

        // Deduplication helpers
        Task<List<Concept>> GetByNormalizedNameAsync(Guid courseId, string normalizedName, CancellationToken ct = default);
        Task<bool> ConceptExistsAsync(Guid courseId, string normalizedName, CancellationToken ct = default);
    }
}
