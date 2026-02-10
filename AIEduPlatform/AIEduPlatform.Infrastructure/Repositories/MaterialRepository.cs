using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Materials;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AIEduPlatform.Infrastructure.Repositories
{
    public class MaterialRepository : GenericRepository<Material>, IMaterialRepository
    {
        private readonly AppDbContext _ctx;
        public MaterialRepository(AppDbContext context) : base(context)
        {
            _ctx = context;
        }
        public async Task<int> DeleteByIdAsync(Guid materialId, CancellationToken ct = default)
        {
            return await _ctx.Materials
                 .Where(e => e.Id == materialId)
                 .ExecuteDeleteAsync(ct);
        }
        public async Task<Material?> GetMaterialByIdAsync(Guid materialId, bool includeChunks = false, CancellationToken ct = default)
        {
            if (!includeChunks)
            {
                return await _ctx.Materials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == materialId, ct);
            }

            return await _ctx.Materials
                .AsNoTracking()
                .Include(m => m.Chunks)
                .FirstOrDefaultAsync(m => m.Id == materialId, ct);
        }
        public async Task AddRangeOfMaterialChunksAsync(IEnumerable<MaterialChunk> chunks, Guid materialId, CancellationToken ct = default)
        {
            await _ctx.Chunks.AddRangeAsync(chunks, ct);
            var material = await _context.Materials
                    .FirstOrDefaultAsync(m => m.Id == materialId, ct);

            if (material == null)
                throw new KeyNotFoundException($"Material {materialId} not found");

            material.Indexed = true;
            await _ctx.SaveChangesAsync();
        }
        public async Task AddMaterialChunksAsync(MaterialChunk chunk, CancellationToken ct = default)
        {
            await _ctx.Chunks.AddAsync(chunk, ct);
            await _ctx.SaveChangesAsync();
        }
        public async Task<Material?> GetMaterialByLectureIdAsync(Guid lectureId, bool includeChunks = false, CancellationToken ct = default)
        {
            if (!includeChunks)
            {
                return await _ctx.Materials
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.LectureId == lectureId, ct);
            }

            return await _ctx.Materials
                .AsNoTracking()
                .Include(m => m.Chunks)
                .FirstOrDefaultAsync(m => m.LectureId == lectureId, ct);
        }
        public async Task<List<Material>> GetMaterialByTitleAsync(string title, bool includeChunks = false, CancellationToken ct = default)
        {
            if (!includeChunks)
            {
                return await _ctx.Materials
                    .AsNoTracking()
                    .Where(m => m.Title == title)
                    .ToListAsync(ct);
            }

            return await _ctx.Materials
                .AsNoTracking()
                .Include(m => m.Chunks)
                .Where(m => m.Title == title)
                .ToListAsync(ct);
        }



        public async Task<MaterialSearchResult?> SearchByEmbeddingAndTextAsync(
            Vector queryEmbedding,
            string keyword,
            int top = 5,
            CancellationToken ct = default)
        {
            // Use raw SQL for optimal performance with pgvector
            var sql = @"
WITH ranked_chunks AS (
    SELECT 
        c.""Id"" as chunk_id,
        c.""MaterialId"",
        c.""Content"",
        c.""Embedding"",
        c.""Section"",
        c.""LectureName"",
        c.""CourseName"",
        c.""PageOrTimestamp"",
        c.""Embedding"" <=> @p0 as distance,
        ROW_NUMBER() OVER (PARTITION BY c.""MaterialId"" ORDER BY c.""Embedding"" <=> @p0) as rn
    FROM ""MaterialChunks"" c
    WHERE c.""Content"" ILIKE @p1
),
best_material AS (
    SELECT ""MaterialId"", MIN(distance) as min_distance
    FROM ranked_chunks
    WHERE rn <= @p2
    GROUP BY ""MaterialId""
    ORDER BY min_distance
    LIMIT 1
)
SELECT 
    bm.""MaterialId"",
    rc.chunk_id,
    rc.""Content"",
    rc.""Embedding"",
    rc.""Section"",
    rc.""LectureName"",
    rc.""CourseName"",
    rc.""PageOrTimestamp"",
    1 - rc.distance AS similarity
FROM best_material bm
INNER JOIN ranked_chunks rc 
    ON rc.""MaterialId"" = bm.""MaterialId"" AND rc.rn <= @p2
ORDER BY rc.distance;
";

            await using var command = _ctx.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            // Parameters
            var p0 = command.CreateParameter();
            p0.ParameterName = "@p0";
            p0.Value = queryEmbedding;
            command.Parameters.Add(p0);

            var p1 = command.CreateParameter();
            p1.ParameterName = "@p1";
            p1.Value = $"%{keyword}%";
            command.Parameters.Add(p1);

            var p2 = command.CreateParameter();
            p2.ParameterName = "@p2";
            p2.Value = top;
            command.Parameters.Add(p2);

            await _ctx.Database.OpenConnectionAsync(ct);

            Guid? materialId = null;
            var topChunks = new List<SearchedChunk>();

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (materialId == null)
                {
                    materialId = reader.GetGuid(0);
                }

                var chunk = new MaterialChunk
                {
                    Id = reader.GetGuid(1),
                    Content = reader.GetString(2),
                    Embedding = reader.GetFieldValue<Vector>(3),
                    Section = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LectureName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CourseName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PageOrTimestamp = reader.IsDBNull(7) ? null : reader.GetString(7),
                    MaterialId = materialId.Value
                };

                var similarity = reader.GetDouble(8); 

                topChunks.Add(new SearchedChunk
                {
                    Chunk = chunk,
                    SimilarityScore = (float)similarity
                });
            }

            if (materialId == null)
                return null;

            return new MaterialSearchResult
            {
                MaterialId = materialId.Value,
                TopChunks = topChunks
            };
        }
        public async Task<MaterialSearchResult?> SearchChunksByMaterialAsync(Guid materialId, Vector queryEmbedding, int top = 5, CancellationToken ct = default)
        {
            var sql = @"
        SELECT 
            c.""MaterialId"",
            c.""Id"" as chunk_id, c.""Content"", c.""Embedding"", c.""Section"", c.""LectureName"", c.""CourseName"", c.""PageOrTimestamp"", 1 - (c.""Embedding"" <=> @p1) AS similarity
        FROM ""MaterialChunks"" c
        WHERE c.""MaterialId"" = @p0
        ORDER BY c.""Embedding"" <=> @p1
        LIMIT @p2;
    ";

            using var command = _ctx.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var p0 = command.CreateParameter();
            p0.ParameterName = "@p0";
            p0.Value = materialId;
            command.Parameters.Add(p0);

            var p1 = command.CreateParameter();
            p1.ParameterName = "@p1";
            p1.Value = queryEmbedding;
            command.Parameters.Add(p1);

            var p2 = command.CreateParameter();
            p2.ParameterName = "@p2";
            p2.Value = top;
            command.Parameters.Add(p2);

            await _ctx.Database.OpenConnectionAsync(ct);

            Guid? foundMaterialId = null;
            var chunks = new List<SearchedChunk>();

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                if (foundMaterialId == null)
                {
                    foundMaterialId = reader.GetGuid(0);
                }

                var chunk = new MaterialChunk
                {
                    Id = reader.GetGuid(1),
                    Content = reader.GetString(2),
                    Embedding = reader.GetFieldValue<Vector>(3),
                    Section = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LectureName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CourseName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PageOrTimestamp = reader.IsDBNull(7) ? null : reader.GetString(7),
                    MaterialId = foundMaterialId.Value
                };
                var similarity = reader.GetDouble(8);
                chunks.Add(new SearchedChunk
                {
                    Chunk = chunk,
                    SimilarityScore = (float)similarity
                });
            }

            if (foundMaterialId == null)
                return null;

            return new MaterialSearchResult
            {
                MaterialId = foundMaterialId.Value,
                TopChunks = chunks
            };
        }
        public async Task<List<MaterialSearchResult>> SearchMaterialsByEmbeddingAsync(
     Vector queryEmbedding,
     int topChunksPerMaterial = 3,
     CancellationToken ct = default)
        {
            var sql = @"
WITH material_chunks AS (
    SELECT 
        m.""Id"" as material_id,
        c.""Id"" as chunk_id,
        c.""Content"",
        c.""Embedding"",
        c.""Section"",
        c.""LectureName"",
        c.""CourseName"",
        c.""PageOrTimestamp"",
        c.""Embedding"" <=> @p0 as distance,
        ROW_NUMBER() OVER (PARTITION BY m.""Id"" ORDER BY c.""Embedding"" <=> @p0) as rn
    FROM ""Materials"" m
    INNER JOIN ""MaterialChunks"" c ON c.""MaterialId"" = m.""Id""
),
ranked_materials AS (
    SELECT 
        material_id,
        MIN(distance) as min_distance
    FROM material_chunks
    WHERE rn <= @p1
    GROUP BY material_id
)
SELECT 
    mc.material_id,
    mc.chunk_id,
    mc.""Content"",
    mc.""Embedding"",
    mc.""Section"",
    mc.""LectureName"",
    mc.""CourseName"",
    mc.""PageOrTimestamp"",
    1 - mc.distance AS similarity
FROM ranked_materials rm
INNER JOIN material_chunks mc 
    ON mc.material_id = rm.material_id AND mc.rn <= @p1
ORDER BY rm.min_distance, mc.material_id, mc.distance;
";

            await using var command = _ctx.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var p0 = command.CreateParameter();
            p0.ParameterName = "@p0";
            p0.Value = queryEmbedding;
            command.Parameters.Add(p0);

            var p1 = command.CreateParameter();
            p1.ParameterName = "@p1";
            p1.Value = topChunksPerMaterial;
            command.Parameters.Add(p1);

            await _ctx.Database.OpenConnectionAsync(ct);

            var resultsDict = new Dictionary<Guid, MaterialSearchResult>();

            await using var reader = await command.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var materialId = reader.GetGuid(0);

                if (!resultsDict.TryGetValue(materialId, out var result))
                {
                    result = new MaterialSearchResult
                    {
                        MaterialId = materialId,
                        TopChunks = new List<SearchedChunk>()
                    };
                    resultsDict[materialId] = result;
                }

                var chunk = new MaterialChunk
                {
                    Id = reader.GetGuid(1),
                    Content = reader.GetString(2),
                    Embedding = reader.GetFieldValue<Vector>(3),
                    Section = reader.IsDBNull(4) ? null : reader.GetString(4),
                    LectureName = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CourseName = reader.IsDBNull(6) ? null : reader.GetString(6),
                    PageOrTimestamp = reader.IsDBNull(7) ? null : reader.GetString(7),
                    MaterialId = materialId
                };

                var similarity = reader.GetDouble(8); 

                result.TopChunks.Add(new SearchedChunk
                {
                    Chunk = chunk,
                    SimilarityScore = (float)similarity
                });
            }

            return resultsDict.Values.ToList();
        }

        public async Task<List<Material>> SearchMaterialsBySummaryAsync(string summary, bool includeChunks = false, CancellationToken ct = default)
        {
            // Use ILIKE for case-insensitive search (indexable with pg_trgm)
            if (!includeChunks)
            {
                return await _ctx.Materials
                    .AsNoTracking()
                    .Where(m => EF.Functions.ILike(m.Summary, $"%{summary}%"))
                    .ToListAsync(ct);
            }

            return await _ctx.Materials
                .AsNoTracking()
                .Include(m => m.Chunks)
                .Where(m => EF.Functions.ILike(m.Summary, $"%{summary}%"))
                .ToListAsync(ct);
        }
        public async Task<List<Material>> SearchMaterialsByTypeAsync(MaterialType type, bool includeChunks = false, CancellationToken ct = default)
        {
            if (!includeChunks)
            {
                return await _ctx.Materials
                    .AsNoTracking()
                    .Where(m => m.Type == type)
                    .ToListAsync(ct);
            }

            return await _ctx.Materials
                .AsNoTracking()
                .Include(m => m.Chunks)
                .Where(m => m.Type == type)
                .ToListAsync(ct);
        }
        public async Task<List<Material>> GetMaterialsToIndexAsync(
              Guid courseId,
              bool reindex,
              CancellationToken cancellationToken)
        {
            var query = _ctx.Materials
                .Include(m => m.Lecture)
                .Where(m => m.Lecture.CourseId == courseId);

            if (!reindex)
            {
                query = query.Where(m => !m.Indexed);
            }

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<List<Material>> GetMaterialsForRetrievalAsync(
            Guid courseId,
            List<Guid>? lectureIds,
            List<Guid>? materialIds,
            List<MaterialType>? materialTypes,
            CancellationToken cancellationToken)
        {
            var query = _ctx.Materials
                .Include(m => m.Lecture)
                .Where(m => m.Lecture.CourseId == courseId);

            if (lectureIds != null && lectureIds.Any())
            {
                query = query.Where(m => lectureIds.Contains(m.LectureId));
            }

            if (materialIds != null && materialIds.Any())
            {
                query = query.Where(m => materialIds.Contains(m.Id));
            }
            if (materialTypes != null && materialTypes.Any())
            {
                query = query.Where(m => materialTypes.Contains(m.Type));
            }

            return await query.ToListAsync(cancellationToken);
        }
        public async Task<bool> HasUnindexedMaterialsAsync(
            Guid courseId,
            CancellationToken cancellationToken)
        {
            return await _ctx.Materials
                .AnyAsync(m => m.Lecture.CourseId == courseId && !m.Indexed, cancellationToken);
        }
        public async Task<bool> HasUnindexedMaterialsInScopeAsync(
            Guid courseId,
            List<Guid>? lectureIds,
            List<Guid>? materialIds,
            CancellationToken cancellationToken)
        {
            var query = _ctx.Materials
                .Where(m => m.Lecture.CourseId == courseId && !m.Indexed);

            if (lectureIds != null && lectureIds.Any())
            {
                query = query.Where(m => lectureIds.Contains(m.LectureId));
            }

            if (materialIds != null && materialIds.Any())
            {
                query = query.Where(m => materialIds.Contains(m.Id));
            }

            return await query.AnyAsync(cancellationToken);
        }
        public async Task<int> GetMaterialsCountAsync(
            Guid courseId,
            CancellationToken cancellationToken)
        {
            return await _ctx.Materials
                .CountAsync(m => m.Lecture.CourseId == courseId, cancellationToken);
        }

    }
}