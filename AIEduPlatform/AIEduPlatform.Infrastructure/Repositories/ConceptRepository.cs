using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class ConceptRepository : GenericRepository<Concept>, IConceptRepository
    {
        private readonly AppDbContext _ctx;

        public ConceptRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }

        // -------------------------------------------------------------------------
        // Write operations
        // -------------------------------------------------------------------------

        public async Task AddConceptsAsync(IEnumerable<Concept> concepts, CancellationToken ct = default)
        {
            await _ctx.Concepts.AddRangeAsync(concepts, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task AddRelationsAsync(IEnumerable<ConceptRelation> relations, CancellationToken ct = default)
        {
            await _ctx.ConceptRelations.AddRangeAsync(relations, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task AddChunkMapsAsync(IEnumerable<ConceptChunkMap> maps, CancellationToken ct = default)
        {
            await _ctx.ConceptChunkMaps.AddRangeAsync(maps, ct);
            await _ctx.SaveChangesAsync(ct);
        }

        public async Task DeleteByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        {
            var conceptIds = await _ctx.Concepts
                .Where(c => c.CourseId == courseId)
                .Select(c => c.Id)
                .ToListAsync(ct);

            if (!conceptIds.Any()) return;

            await _ctx.ConceptRelations
                .Where(r => conceptIds.Contains(r.FromConceptId)
                         || conceptIds.Contains(r.ToConceptId))
                .ExecuteDeleteAsync(ct);

            await _ctx.ConceptChunkMaps
                .Where(m => conceptIds.Contains(m.ConceptId))
                .ExecuteDeleteAsync(ct);

            await _ctx.Concepts
                .Where(c => c.CourseId == courseId)
                .ExecuteDeleteAsync(ct);

        }

        // -------------------------------------------------------------------------
        // Lookup
        // -------------------------------------------------------------------------

        public async Task<List<Concept>> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        {
            return await _ctx.Concepts
                .AsNoTracking()
                .Where(c => c.CourseId == courseId)
                .Include(c => c.OutgoingRelations)
                .ToListAsync(ct);
        }

        public async Task<List<Concept>> FindByNormalizedNamesAsync(
            Guid courseId,
            IEnumerable<string> normalizedNames,
            CancellationToken ct = default)
        {
            var nameList = normalizedNames.ToList();

            return await _ctx.Concepts
                .AsNoTracking()
                .Where(c => c.CourseId == courseId && nameList.Contains(c.NormalizedName))
                .ToListAsync(ct);
        }

        public async Task<List<Concept>> SearchByEmbeddingAsync(
            Guid courseId,
            Vector queryEmbedding,
            int topK = 10,
            CancellationToken ct = default)
        {
            // Raw SQL for pgvector cosine similarity
            var sql = @"
SELECT c.*
FROM ""Concepts"" c
WHERE c.""CourseId"" = @p0
ORDER BY c.""Embedding"" <=> @p1
LIMIT @p2";

            return await _ctx.Concepts
                .FromSqlRaw(sql, courseId, queryEmbedding, topK)
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task<List<Concept>> GetByNormalizedNameAsync(
            Guid courseId,
            string normalizedName,
            CancellationToken ct = default)
        {
            return await _ctx.Concepts
                .AsNoTracking()
                .Where(c => c.CourseId == courseId && c.NormalizedName == normalizedName)
                .ToListAsync(ct);
        }

        public async Task<bool> ConceptExistsAsync(
            Guid courseId,
            string normalizedName,
            CancellationToken ct = default)
        {
            return await _ctx.Concepts
                .AnyAsync(c => c.CourseId == courseId && c.NormalizedName == normalizedName, ct);
        }

        // -------------------------------------------------------------------------
        // Graph traversal — BFS up to `depth` hops via ConceptRelations
        // -------------------------------------------------------------------------

        public async Task<List<Guid>> GetNeighbourConceptIdsAsync(
            IEnumerable<Guid> conceptIds,
            int depth = 2,
            CancellationToken ct = default)
        {
            var visited = new HashSet<Guid>(conceptIds);
            var frontier = new HashSet<Guid>(conceptIds);

            for (int i = 0; i < depth; i++)
            {
                if (!frontier.Any()) break;

                var frontierList = frontier.ToList();

                var edges = await _ctx.ConceptRelations
                    .AsNoTracking()
                    .Where(r => frontierList.Contains(r.FromConceptId)
                             || frontierList.Contains(r.ToConceptId))
                    .Select(r => new { r.FromConceptId, r.ToConceptId })
                    .ToListAsync(ct);

                frontier.Clear();

                foreach (var edge in edges)
                {
                    if (visited.Add(edge.FromConceptId)) frontier.Add(edge.FromConceptId);
                    if (visited.Add(edge.ToConceptId)) frontier.Add(edge.ToConceptId);
                }
            }

            return visited.ToList();
        }
        // -------------------------------------------------------------------------
        // Chunk resolution
        // -------------------------------------------------------------------------

        public async Task<List<Guid>> GetChunkIdsByConceptIdsAsync(
            IEnumerable<Guid> conceptIds,
            CancellationToken ct = default)
        {
            var idList = conceptIds.ToList();

            return await _ctx.ConceptChunkMaps
                .AsNoTracking()
                .Where(m => idList.Contains(m.ConceptId))
                .Select(m => m.ChunkId)
                .Distinct()
                .ToListAsync(ct);
        }
    }
}
