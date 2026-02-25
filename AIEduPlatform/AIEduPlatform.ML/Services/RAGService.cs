using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Materials;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Services.Material_Processing;
using AIEduPlatform.ML.Services.RAG;
using AIEduPlatform.ML.Services.Utilities;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AIEduPlatform.ML.Services
{
    public sealed class RetrievalContext
    {
        public long EmbeddingTimeMs { get; set; }
        public long SearchTimeMs { get; set; }
        public long RerankTimeMs { get; set; }
        public int GraphExpansionChunks { get; set; }
        public QueryIntelligenceResult Intelligence { get; set; } = null!;

        public RetrievalMetadata ToMetadata() => new()
        {
            EmbeddingTimeMs = EmbeddingTimeMs,
            SearchTimeMs = SearchTimeMs,
            RerankTimeMs = RerankTimeMs,
            GraphExpansionChunks = GraphExpansionChunks,
            RewrittenQuery = Intelligence.RewrittenQuery,
            FallbackUsed = Intelligence.FallbackUsed,
            TargetConcepts = Intelligence.TargetConcepts
        };
    }
    public class RAGService : IRAGService
    {
        private readonly IUnitOfWork _uow;
        private readonly DocumentIndexingHelper _documentIndexer;
        private readonly AudioIndexingHelper _audioIndexer;
        private readonly ImageIndexingHelper _imageIndexer;
        private readonly IEmbeddingService _embeddingService;
        private readonly IRerankingService _rerankingService;
        private readonly IGraphMergeService _graphMergeService;
        private readonly IQueryIntelligenceService _queryIntelligenceService;
        private readonly VideoIndexingHelper _videoIndexer;
        private readonly ILogger<RAGService> _logger;

        private readonly SemaphoreSlim _rerankingSemaphore;


        private readonly RagSettings _ragSettings;

        public RAGService(
            IUnitOfWork uow,
            DocumentIndexingHelper documentIndexer,
            AudioIndexingHelper audioIndexer,
            IEmbeddingService embeddingService,
            IRerankingService rerankingService,
            IOptions<RagSettings> options,
            ILogger<RAGService> logger,
            ImageIndexingHelper imageIndexer,
            VideoIndexingHelper videoIndexer,
            IRerankConcurrencyLimiter rerankConcurrencyLimiter,
            IQueryIntelligenceService queryIntelligenceService,
            IGraphMergeService graphMergeService)
        {
            _uow = uow;
            _documentIndexer = documentIndexer;
            _audioIndexer = audioIndexer;
            _imageIndexer = imageIndexer;
            _graphMergeService = graphMergeService;
            _embeddingService = embeddingService;
            _rerankingService = rerankingService;
            _logger = logger;
            _ragSettings = options.Value;
            _videoIndexer = videoIndexer;
            _queryIntelligenceService = queryIntelligenceService;

            _rerankingSemaphore = rerankConcurrencyLimiter.Semaphore;

            _logger.LogInformation(
                "RAGService initialized with settings: " +
                "MaxRetries={MaxRetries}, EmbeddingDelayMs={DelayMs}",
                _ragSettings.MaxRetryAttempts,
                _ragSettings.EmbeddingDelayMs);
        }

        public async Task<RagIndexResponse> IndexAsync(
             RagIndexRequest request,
             CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("IndexAsync started: CourseId={CourseId}, Reindex={Reindex}",
                request.CourseId, request.Reindex);

            var courseExists = await _uow.Courses.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
            {
                _logger.LogWarning("IndexAsync failed: course not found. CourseId={CourseId}", request.CourseId);
                throw new KeyNotFoundException("Course not found");
            }

            var totalMaterialsCount = await _uow.Materials.GetMaterialsCountAsync(
                request.CourseId, cancellationToken);
            if (totalMaterialsCount == 0)
            {
                _logger.LogWarning("IndexAsync failed: course has no materials. CourseId={CourseId}",
                    request.CourseId);
                throw new InvalidOperationException($"Course {request.CourseId} has no materials");
            }

            if (!request.Reindex)
            {
                var hasUnindexed = await _uow.Materials.HasUnindexedMaterialsAsync(
                    request.CourseId, cancellationToken);
                if (!hasUnindexed)
                {
                    _logger.LogInformation(
                        "IndexAsync skipped: no unindexed materials. CourseId={CourseId}", request.CourseId);

                    return new RagIndexResponse
                    {
                        Success = true,
                        Error = "No materials to index",
                        ChunksIndexed = 0,
                        ChunksFailed = 0,
                        FailureRatio = 0,
                        CourseId = request.CourseId,
                        IndexTimeMs = 0,
                        EmbeddingTimeMs = 0
                    };
                }
            }

            var course = await _uow.Courses.GetCourseByIdAsync(
                request.CourseId,
                new CourseIncludeOptions { IncludeMaterials = false },
                cancellationToken);

            var materialsToIndex = await _uow.Materials.GetMaterialsToIndexAsync(
                request.CourseId,
                request.Reindex,
                cancellationToken);

            _logger.LogInformation(
                "IndexAsync: materials to index identified. CourseId={CourseId}, MaterialCount={Count}",
                request.CourseId, materialsToIndex.Count);

            int numOfChunksIndexed = 0;
            long totalEmbeddingMs = 0;
            int failedChunks = 0;

            var totalStopwatch = Stopwatch.StartNew();
            var maxConcurrent = _ragSettings.Concurrency.MaxConcurrentMaterials;
            var semaphore = new SemaphoreSlim(maxConcurrent);

            var tasks = materialsToIndex.Select(async material =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await IndexMaterialByTypeAsync(request, material, course, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            // ── Aggregate results ────────────────────────────────────────────────
            try
            {
                var results = await Task.WhenAll(tasks);

                numOfChunksIndexed = results.Sum(r => r.numOfChunksIndexed);
                totalEmbeddingMs = results.Sum(r => r.totalEmbeddingMs);
                failedChunks = results.Sum(r => r.failedChunks);

                // ── Graph merge pass (non-fatal) ─────────────────────────────────
                var allExtractions = results
                    .SelectMany(r => r.conceptExtractions)
                    .ToList();

                if (allExtractions.Any(e => e.Concepts.Any()))
                {
                    _logger.LogInformation(
                        "IndexAsync: starting graph merge. CourseId={CourseId}, " +
                        "TotalExtractions={Total}, ConceptsFound={Concepts}",
                        request.CourseId,
                        allExtractions.Count,
                        allExtractions.Sum(e => e.Concepts.Count));

                    try
                    {
                        await _graphMergeService.MergeAndStoreGraphAsync(
                            request.CourseId,
                            materialsToIndex.First().Id,
                            allExtractions,
                            _embeddingService,
                            cancellationToken);

                        _logger.LogInformation(
                            "IndexAsync: graph merge completed. CourseId={CourseId}", request.CourseId);
                    }
                    catch (Exception ex)
                    {
                        // Graph failure must never break indexing — vector retrieval remains functional
                        _logger.LogError(ex,
                            "IndexAsync: graph merge failed for course {CourseId} — " +
                            "vector retrieval is still fully functional", request.CourseId);
                    }
                }
                else
                {
                    _logger.LogInformation(
                        "IndexAsync: no concepts extracted, skipping graph merge. CourseId={CourseId}",
                        request.CourseId);
                }
            }
            catch (Exception ex)
            {
                var failedMaterials = materialsToIndex
                    .Zip(tasks, (material, task) => new { material, task })
                    .Where(x => x.task.IsFaulted)
                    .Select(x => new
                    {
                        x.material.Id,
                        x.material.Title,
                        Error = x.task.Exception?.GetBaseException().Message
                    })
                    .ToList();

                _logger.LogError("IndexAsync: failed materials: {@FailedMaterials}", failedMaterials);

                throw new AggregateException(
                    $"Failed to index {failedMaterials.Count} materials: " +
                    $"{string.Join(", ", failedMaterials.Select(m => m.Title))}",
                    tasks.Where(t => t.IsFaulted)
                         .Select(t => t.Exception!)
                         .ToList());
            }

            totalStopwatch.Stop();

            var totalAttempted = numOfChunksIndexed + failedChunks;
            var failureRatio = totalAttempted > 0
                ? (double)failedChunks / totalAttempted  // ← fixed denominator
                : 0;

            _logger.LogInformation(
                "IndexAsync completed: CourseId={CourseId}, ChunksIndexed={Indexed}, " +
                "ChunksFailed={Failed}, FailureRatio={Ratio:P2}, " +
                "TotalTimeMs={TotalMs}, EmbeddingTimeMs={EmbedMs}",
                request.CourseId, numOfChunksIndexed, failedChunks,
                failureRatio, totalStopwatch.ElapsedMilliseconds, totalEmbeddingMs);

            return new RagIndexResponse
            {
                Success = true,
                Error = string.Empty,
                ChunksIndexed = numOfChunksIndexed,
                ChunksFailed = failedChunks,
                FailureRatio = failureRatio,
                CourseId = course.Id,
                IndexTimeMs = totalStopwatch.ElapsedMilliseconds,
                EmbeddingTimeMs = totalEmbeddingMs
            };
        }
        public async Task<RagRetrievalResponse> RetrieveAsync(RagRetrievalRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("RetrieveAsync started: CourseId={CourseId}, LectureIds={LectureIds}, MaterialIds={MaterialIds}, TopK={TopK}, FinalTopK={FinalTopK}, UseReranking={Rerank}",
                request.CourseId,
                request.LectureIds != null ? string.Join(",", request.LectureIds) : "null",
                request.MaterialIds != null ? string.Join(",", request.MaterialIds) : "null",
                request.TopK, request.FinalTopK, request.UseReranking);

            var ctx = new RetrievalContext();
            var totalStopwatch = Stopwatch.StartNew();

            await ValidateRequestAsync(request, ct);
            await EnsureMaterialsIndexedAsync(request, ct);

            var materials = await LoadMaterialsAsync(request, ct);

            var materialContexts = materials.Select(m => new MaterialContext
            {
                Id = m.Id,
                Title = m.Title,
                Type = m.Type.ToString(),
                Summary = m.Summary ?? string.Empty
            }).ToList();

            _logger.LogDebug("RetrieveAsync: loaded {Count} material(s) for search. CourseId={CourseId}",
                materials.Count, request.CourseId);

            ctx.Intelligence = await _queryIntelligenceService.AnalyzeAsync(
                request.Query,
                request.ConversationHistory,
                materialContexts,
                ct);

            if (ctx.Intelligence.Intent == QueryIntent.Conversational)
            {
                totalStopwatch.Stop();
                return new RagRetrievalResponse
                {
                    Success = true,
                    Intent = ctx.Intelligence.Intent,
                    Query = request.Query,
                    Chunks = new List<ContextChunk>(),
                    TotalFound = 0,
                    RerankingApplied = false,
                    Error = string.Empty,
                    RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                    Metadata = ctx.ToMetadata(),
                    TargetMaterialIds = null
                };
            }

            List<Material> searchMaterials;
            if (ctx.Intelligence.TargetMaterialIds?.Any() == true)
            {
                var targeted = materials
                    .Where(m => ctx.Intelligence.TargetMaterialIds.Contains(m.Id))
                    .ToList();

                searchMaterials = targeted.Any() ? targeted : materials;

                _logger.LogInformation(
                    "RetrieveAsync: scoped search to MaterialIds=[{MaterialIds}]",
                    string.Join(", ", ctx.Intelligence.TargetMaterialIds));
            }
            else
            {
                searchMaterials = materials;
            }

            var embeddedQuery = await EmbedQueryWithRetryAsync(ctx.Intelligence.RewrittenQuery, ctx, ct);

            var searchedChunks = await SearchChunksAsync(searchMaterials, embeddedQuery, request.TopK, ctx, ct);
            if (request.UseGraphExpansion && ctx.Intelligence.TargetConcepts.Any())
            {
                var graphChunks = await ExpandViaGraphAsync(request.CourseId, ctx.Intelligence.TargetConcepts, ct);

                var existingIds = searchedChunks.Select(c => c.Chunk.Id).ToHashSet();
                var newGraphChunks = graphChunks
                    .Where(c => !existingIds.Contains(c.Chunk.Id))
                    .ToList();

                searchedChunks.AddRange(newGraphChunks);
                ctx.GraphExpansionChunks = newGraphChunks.Count;

                _logger.LogInformation(
                    "RetrieveAsync: graph expansion added {Count} new chunks. Total pool now {Total}",
                    newGraphChunks.Count, searchedChunks.Count);
            }

            var totalFound = searchedChunks.Count;

            _logger.LogInformation("RetrieveAsync: vector search returned {TotalFound} chunk(s). CourseId={CourseId}, SearchTimeMs={SearchMs}",
                totalFound, request.CourseId, ctx.SearchTimeMs);

            var result = request.UseReranking
                ? await TryRerankAsync(request, searchedChunks, totalStopwatch, totalFound, ctx, ct)
                : null;

            if (result != null)
            {
                _logger.LogInformation("RetrieveAsync completed (reranked): CourseId={CourseId}, TotalFound={TotalFound}, FinalChunks={Final}, " +
                    "EmbeddingTimeMs={EmbedMs}, SearchTimeMs={SearchMs}, RerankTimeMs={RerankMs}, TotalTimeMs={TotalMs}",
                    request.CourseId, totalFound, result.Chunks.Count,
                    ctx.EmbeddingTimeMs, ctx.SearchTimeMs, ctx.RerankTimeMs, result.RetrievalTimeMs);
                result.Intent = ctx.Intelligence.Intent;
                result.TargetMaterialIds = ctx.Intelligence.TargetMaterialIds;
                return result;
            }

            var nonRerankedResult = await BuildNonRerankedResponse(request, searchedChunks, totalFound, totalStopwatch, ctx);

            _logger.LogInformation("RetrieveAsync completed (no reranking): CourseId={CourseId}, TotalFound={TotalFound}, FinalChunks={Final}, " +
                "EmbeddingTimeMs={EmbedMs}, SearchTimeMs={SearchMs}, TotalTimeMs={TotalMs}",
                request.CourseId, totalFound, nonRerankedResult.Chunks.Count,
                ctx.EmbeddingTimeMs, ctx.SearchTimeMs, nonRerankedResult.RetrievalTimeMs);

            nonRerankedResult.Intent = ctx.Intelligence.Intent;
            nonRerankedResult.TargetMaterialIds = ctx.Intelligence.TargetMaterialIds; 
            return nonRerankedResult;
        }

        public async Task<RagIndexStats> GetIndexStatsAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Fetching index stats: CourseId={CourseId}", courseId);

            var course = await _uow.Courses.GetCourseByIdAsync(courseId, new CourseIncludeOptions { IncludeMaterials = true }, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found when fetching index stats: CourseId={CourseId}", courseId);
                throw new KeyNotFoundException("Course not found");
            }

            var result = new RagIndexStats
            {
                CourseId = courseId,
                CourseName = course?.Title ?? "Unknown",
                TotalLectures = course?.Lectures?.Count ?? 0,
                TotalMaterials = course?.Lectures?.Sum(e => e.Materials?.Count ?? 0) ?? 0,
                TotalChunks = course?.Lectures?.Sum(l => l.Materials?.Sum(m => m.Chunks?.Count ?? 0) ?? 0) ?? 0,
                ByLecture = course?.Lectures?.Select(e => new LectureIndexStats
                {
                    LectureId = e.Id,
                    LectureName = e.Title,
                    MaterialCount = e.Materials.Count,
                    ChunkCount = e.Materials.Sum(e => e.Chunks.Count),
                    Materials = e.Materials.Select(x => new MaterialIndexInfo
                    {
                        MaterialId = x.Id,
                        Title = x.Title,
                        MaterialType = x.Type.ToString(),
                        ChunkCount = x.Chunks.Count,
                        IndexedAt = x.CreatedAt,
                        IsComplete = true
                    }).ToList()
                }).ToList() ?? new List<LectureIndexStats>(),
                LastIndexedAt = course.UpdatedAt,
                EstimatedTokenCount = course?.Lectures?
                    .SelectMany(l => l.Materials ?? Enumerable.Empty<Material>())
                    .SelectMany(m => m.Chunks ?? Enumerable.Empty<MaterialChunk>())
                    .Sum(c => string.IsNullOrWhiteSpace(c.Content) ? 0 : c.Content.Length / 4) ?? 0,
                ByMaterialType = course?.Lectures?
                    .SelectMany(l => l.Materials ?? Enumerable.Empty<Material>())
                    .GroupBy(m => m.Type)
                    .ToDictionary(
                        g => g.Key.ToString(),
                        g => new MaterialTypeStats
                        {
                            MaterialType = g.Key.ToString(),
                            MaterialCount = g.Count(),
                            ChunkCount = g.Sum(m => m.Chunks?.Count ?? 0)
                        }
                        ) ?? new Dictionary<string, MaterialTypeStats>()
            };

            _logger.LogInformation("Index stats retrieved: CourseId={CourseId}, Lectures={Lectures}, Materials={Materials}, Chunks={Chunks}, EstimatedTokens={Tokens}",
                courseId, result.TotalLectures, result.TotalMaterials, result.TotalChunks, result.EstimatedTokenCount);

            return result;
        }
        public async Task<bool> IsMaterialIndexedAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IsMaterialIndexedAsync: MaterialId={MaterialId}", materialId);

            var result = await _uow.Materials.GetMaterialByIdAsync(materialId, includeChunks: false, cancellationToken);
            if (result == null)
            {
                _logger.LogWarning("IsMaterialIndexedAsync: material not found. MaterialId={MaterialId}", materialId);
                throw new KeyNotFoundException("Material Not Found");
            }

            _logger.LogDebug("IsMaterialIndexedAsync: MaterialId={MaterialId}, Indexed={Indexed}", materialId, result.Indexed);
            return result.Indexed;
        }
        public async Task<int> GetChunkCountAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching chunk count: MaterialId={MaterialId}", materialId);

            var fetched = await _uow.Materials.GetMaterialByIdAsync(materialId, includeChunks: true, cancellationToken);
            var count = fetched?.Chunks?.Count ?? 0;

            _logger.LogDebug("Chunk count retrieved: MaterialId={MaterialId}, Count={Count}", materialId, count);

            return count;
        }


        public async Task<RagDeleteResponse> DeleteAsync(RagDeleteRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("DeleteAsync called: MaterialId={MaterialId}, LectureId={LectureId}, CourseId={CourseId}",
                request.MaterialId, request.LectureId, request.CourseId);

            try
            {
                var response = await (request switch
                {
                    { MaterialId: { } materialId } => DeleteMaterialAsync(materialId, cancellationToken),
                    { LectureId: { } lectureId } => DeleteLectureAsync(lectureId, cancellationToken),
                    { CourseId: { } courseId } => DeleteCourseAsync(courseId, cancellationToken),
                    _ => Task.FromResult(new RagDeleteResponse
                    {
                        Success = false,
                        Error = "No valid ID provided"
                    })
                });

                if (!response.Success)
                    _logger.LogWarning("DeleteAsync completed unsuccessfully: Error={Error}", response.Error);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAsync failed unexpectedly: MaterialId={MaterialId}, LectureId={LectureId}, CourseId={CourseId}",
                    request.MaterialId, request.LectureId, request.CourseId);

                return new RagDeleteResponse
                {
                    Success = false,
                    Error = ex.Message,
                };
            }
        }
        public async Task<RagDeleteResponse> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting course: CourseId={CourseId}", courseId);

            try
            {
                var deletedRows = await _uow.Courses.DeleteByIdAsync(courseId, cancellationToken);

                if (deletedRows > 0)
                    _logger.LogInformation("Course deleted successfully: CourseId={CourseId}, RowsDeleted={Rows}", courseId, deletedRows);
                else
                    _logger.LogWarning("Course delete returned zero rows: CourseId={CourseId}", courseId);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete course: CourseId={CourseId}", courseId);

                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete course: {ex.Message}"
                };
            }
        }
        public async Task<RagDeleteResponse> DeleteLectureAsync(Guid lectureId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting lecture: LectureId={LectureId}", lectureId);

            try
            {
                var deletedRows = await _uow.Lectures.DeleteByIdAsync(lectureId, cancellationToken);

                if (deletedRows > 0)
                    _logger.LogInformation("Lecture deleted successfully: LectureId={LectureId}, RowsDeleted={Rows}", lectureId, deletedRows);
                else
                    _logger.LogWarning("Lecture delete returned zero rows: LectureId={LectureId}", lectureId);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete lecture: LectureId={LectureId}", lectureId);

                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete lecture: {ex.Message}"
                };
            }
        }
        public async Task<RagDeleteResponse> DeleteMaterialAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting material: MaterialId={MaterialId}", materialId);

            try
            {
                var deletedRows = await _uow.Materials.DeleteByIdAsync(materialId, cancellationToken);

                if (deletedRows > 0)
                    _logger.LogInformation("Material deleted successfully: MaterialId={MaterialId}, RowsDeleted={Rows}", materialId, deletedRows);
                else
                    _logger.LogWarning("Material delete returned zero rows: MaterialId={MaterialId}", materialId);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete material: MaterialId={MaterialId}", materialId);

                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete material: {ex.Message}"
                };
            }
        }
        private async Task<List<SearchedChunk>> ExpandViaGraphAsync(
            Guid courseId,
            List<string> targetConcepts,
            CancellationToken ct)
        {
            if (!targetConcepts.Any()) return new List<SearchedChunk>();

            // Normalize incoming concept names to match stored NormalizedName
            var normalizedNames = targetConcepts
                .Select(GraphMergeService.Normalize)
                .ToList();

            var seedConcepts = await _uow.Concepts
                .FindByNormalizedNamesAsync(courseId, normalizedNames, ct);

            if (!seedConcepts.Any())
            {
                _logger.LogDebug(
                    "ExpandViaGraphAsync: no concept nodes found for [{Concepts}] in course {CourseId}",
                    string.Join(", ", targetConcepts), courseId);
                return new List<SearchedChunk>();
            }

            var expandedIds = await _uow.Concepts
                .GetNeighbourConceptIdsAsync(seedConcepts.Select(c => c.Id), depth: 2, ct);

            var chunkIds = await _uow.Concepts
                .GetChunkIdsByConceptIdsAsync(expandedIds, ct);

            if (!chunkIds.Any()) return new List<SearchedChunk>();

            var chunks = await _uow.Materials.GetChunksByIdsAsync(chunkIds, ct);

            _logger.LogInformation(
                "ExpandViaGraphAsync: {SeedCount} seed concepts → " +
                "{ExpandedCount} expanded → {ChunkCount} chunks added",
                seedConcepts.Count, expandedIds.Count, chunks.Count);

            return chunks.Select(c => new SearchedChunk
            {
                Chunk = c,
                SimilarityScore = 0.5f // neutral score — reranker will re-sort
            }).ToList();
        }

        private Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks, List<ChunkConceptsResult> conceptExtractions)> IndexMaterialByTypeAsync(
            RagIndexRequest request,
            Material material,
            Course course,
            CancellationToken cancellationToken)
        {
            return material.Type switch
            {
                Core.Domain.Enums.MaterialType.Document =>
                    _documentIndexer.IndexDocumentAsync(course, material, request.options, cancellationToken),

                Core.Domain.Enums.MaterialType.Audio =>
                    _audioIndexer.IndexAudioAsync(course, material, request.options, cancellationToken),

                Core.Domain.Enums.MaterialType.Image =>
                    _imageIndexer.IndexImageAsync(course, material, request.options, cancellationToken),

                Core.Domain.Enums.MaterialType.Video =>
                    _videoIndexer.IndexVideoAsync(course, material, request.options, cancellationToken),

                _ => Task.FromResult((0, 0L, 0, new List<ChunkConceptsResult>()))
            };
        }

        private async Task ValidateRequestAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            _logger.LogDebug("ValidateRequestAsync: CourseId={CourseId}", request.CourseId);

            if (!await _uow.Courses.CourseExistsAsync(request.CourseId, ct))
            {
                _logger.LogWarning("ValidateRequestAsync: course not found. CourseId={CourseId}", request.CourseId);
                throw new KeyNotFoundException("Course not found");
            }

            if (!await _uow.Lectures.CourseHasLecturesAsync(request.CourseId, ct))
            {
                _logger.LogWarning("ValidateRequestAsync: course has no lectures. CourseId={CourseId}", request.CourseId);
                throw new InvalidOperationException($"Course {request.CourseId} has no lectures");
            }

            if (request.LectureIds?.Any() == true)
            {
                var lecturesExist = await _uow.Lectures.LecturesExistInCourseAsync(
                    request.CourseId,
                    request.LectureIds,
                    ct);

                if (!lecturesExist)
                {
                    _logger.LogWarning("ValidateRequestAsync: one or more specified lectures not found. CourseId={CourseId}, LectureIds=[{LectureIds}]",
                        request.CourseId, string.Join(",", request.LectureIds));
                    throw new InvalidOperationException($"One or more specified lectures are not found in Course {request.CourseId}");
                }
            }

            _logger.LogDebug("ValidateRequestAsync: validation passed. CourseId={CourseId}", request.CourseId);
        }
        private async Task EnsureMaterialsIndexedAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            var hasUnindexedMaterials = await _uow.Materials.HasUnindexedMaterialsInScopeAsync(
                request.CourseId,
                request.LectureIds,
                request.MaterialIds,
                ct);

            if (hasUnindexedMaterials)
            {
                _logger.LogInformation("EnsureMaterialsIndexedAsync: unindexed materials detected, triggering indexing. CourseId={CourseId}",
                    request.CourseId);

                await IndexAsync(new RagIndexRequest
                {
                    CourseId = request.CourseId,
                }, ct);
            }
            else
            {
                _logger.LogDebug("EnsureMaterialsIndexedAsync: all materials in scope are indexed. CourseId={CourseId}", request.CourseId);
            }
        }
        private async Task<List<Material>> LoadMaterialsAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            var materials = await _uow.Materials.GetMaterialsForRetrievalAsync(
                request.CourseId,
                request.LectureIds,
                request.MaterialIds,
                request.MaterialTypes,
                ct);

            if (!materials.Any())
            {
                _logger.LogWarning("LoadMaterialsAsync: no materials matched criteria. CourseId={CourseId}, LectureIds={LectureIds}, MaterialIds={MaterialIds}",
                    request.CourseId,
                    request.LectureIds != null ? string.Join(",", request.LectureIds) : "null",
                    request.MaterialIds != null ? string.Join(",", request.MaterialIds) : "null");
                throw new InvalidOperationException($"No materials found matching the specified criteria in Course {request.CourseId}");
            }

            return materials;
        }
        private async Task<EmbeddingResponse> EmbedQueryWithRetryAsync(
     string query,
     RetrievalContext ctx,
     CancellationToken ct)
        {
            _logger.LogDebug("EmbedQueryWithRetryAsync: starting query embedding. QueryLength={Length}", query.Length);

            var stopwatch = Stopwatch.StartNew();
            int attempt = 0;

            while (attempt < _ragSettings.MaxRetryAttempts)
            {
                attempt++;
                try
                {
                    var response = await _embeddingService.GetEmbeddingAsync(
                        new EmbeddingRequest { Text = query }, ct);

                    if (response != null)
                    {
                        stopwatch.Stop();
                        ctx.EmbeddingTimeMs = stopwatch.ElapsedMilliseconds;

                        _logger.LogDebug("EmbedQueryWithRetryAsync: embedded successfully on attempt {Attempt}. EmbeddingTimeMs={Ms}",
                            attempt, ctx.EmbeddingTimeMs);

                        return response;
                    }

                    _logger.LogWarning("EmbedQueryWithRetryAsync: null response on attempt {Attempt}/{Max}.",
                        attempt, _ragSettings.MaxRetryAttempts);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "EmbedQueryWithRetryAsync: failed on attempt {Attempt}/{Max}.",
                        attempt, _ragSettings.MaxRetryAttempts);
                }

                await Task.Delay(_ragSettings.EmbeddingDelayMs, ct);
            }

            _logger.LogError("EmbedQueryWithRetryAsync: exhausted all {Max} retry attempts.", _ragSettings.MaxRetryAttempts);
            throw new Exception($"Failed to embed user query after {_ragSettings.MaxRetryAttempts} attempts.");
        }
        private async Task<ContextChunk> MapToContextChunkAsync(SearchedChunk chunk, MappingRequest request)
        {
            return new ContextChunk
            {
                Content = chunk.Chunk.Content,
                RelevanceScore = request.relevanceScore,
                Metadata = new ChunkMetadata
                {
                    SourceTitle = request?.SourceTitle ?? "Unknown",
                    MaterialType = request?.MaterialType.ToString() ?? "Unknown",
                    CourseName = chunk?.Chunk.CourseName ?? "Unknown",
                    LectureName = chunk?.Chunk.LectureName ?? "Unknown",
                    Section = chunk?.Chunk.Section ?? "Unknown",
                    PageOrTimestamp = chunk?.Chunk.PageOrTimestamp ?? "Unknown",
                    MaterialId = chunk.Chunk.MaterialId,
                },
                AdditionalData = chunk.Chunk.AdditionalData
            };
        }
        public class MappingRequest
        {
            public string SourceTitle;
            public MaterialType MaterialType;
            public float relevanceScore;
        }
        private async Task<List<SearchedChunk>> SearchChunksAsync(
            List<Material> materials,
            EmbeddingResponse embeddedQuery,
            int topK,
            RetrievalContext ctx,
            CancellationToken ct)
        {
            _logger.LogDebug("SearchChunksAsync: starting vector search across {MaterialCount} material(s), TopK={TopK}",
                materials.Count, topK);

            var stopwatch = Stopwatch.StartNew();
            var queryVector = new Vector(embeddedQuery.Embedding.ToArray());
            var searchedChunks = new List<SearchedChunk>();

            foreach (var material in materials)
            {
                var result = await _uow.Materials.SearchChunksByMaterialAsync(
                    material.Id, queryVector, topK, ct);

                if (result?.TopChunks?.Any() == true)
                {
                    _logger.LogDebug("SearchChunksAsync: MaterialId={MaterialId} returned {Count} chunk(s).",
                        material.Id, result.TopChunks.Count);
                    searchedChunks.AddRange(result.TopChunks);
                }
                else
                {
                    _logger.LogDebug("SearchChunksAsync: MaterialId={MaterialId} returned no matching chunks.", material.Id);
                }
            }

            stopwatch.Stop();
            ctx.SearchTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogDebug("SearchChunksAsync completed: TotalChunksFound={Total}, SearchTimeMs={Ms}",
                searchedChunks.Count, ctx.SearchTimeMs);

            return searchedChunks;
        }
        private async Task<RagRetrievalResponse?> TryRerankAsync(
           RagRetrievalRequest request,
           List<SearchedChunk> searchedChunks,
           Stopwatch totalStopwatch,
           int totalFound,
           RetrievalContext ctx,
           CancellationToken ct)
        {
            if (!searchedChunks.Any())
            {
                _logger.LogDebug("TryRerankAsync: no chunks to rerank, skipping.");
                return null;
            }

            _logger.LogInformation("TryRerankAsync: starting reranking. InputChunks={Input}, FinalTopK={FinalTopK}, MinScore={MinScore}",
                searchedChunks.Count, request.FinalTopK, request.MinScore);

            var rerankStopwatch = Stopwatch.StartNew();

            var rerankRequest = new RerankRequest
            {
                Query = ctx.Intelligence.RewrittenQuery,
                Chunks = searchedChunks.Select((chunk, index) => new RerankChunk
                {
                    Index = index,
                    Content = chunk.Chunk.Content
                }).ToList(),
                TopK = request.FinalTopK,
                ReturnContent = true
            };

            RerankResponse response = null;

            await _rerankingSemaphore.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                response = await _rerankingService.RerankAsync(rerankRequest, ct);
                rerankStopwatch.Stop();
                ctx.RerankTimeMs = rerankStopwatch.ElapsedMilliseconds;

                if (response?.Results == null)
                {
                    _logger.LogWarning("TryRerankAsync: reranking service returned null results.");
                    return null;
                }
            }
            finally
            {
                _rerankingSemaphore.Release();
            }

            var topResults = response.Results
                .Where(r => r.Score >= request.MinScore)
                .Where(r => r.Index >= 0 && r.Index < searchedChunks.Count)
                .ToList();

            if (!topResults.Any() && response.Results.Any())
            {
                var bestScore = response.Results.Max(r => r.Score);
                _logger.LogWarning(
                    "TryRerankAsync: all {Total} chunks scored below MinScore={MinScore}. Best={BestScore:F4}. Returning empty.",
                    response.Results.Count, request.MinScore, bestScore);

                totalStopwatch.Stop();
                return new RagRetrievalResponse
                {
                    Success = true,
                    Query = request.Query,
                    Chunks = new List<ContextChunk>(),
                    TotalFound = totalFound,
                    RerankingApplied = true,
                    RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                    Metadata = ctx.ToMetadata()
                };
            }

            var mappingTasks = topResults
                .Select(async r => await MapToContextChunkAsync(
                    searchedChunks[r.Index],
                    new MappingRequest
                    {
                        SourceTitle = searchedChunks[r.Index].Chunk.Material.Title,
                        MaterialType = searchedChunks[r.Index].Chunk.Material.Type,
                        relevanceScore = r.Score
                    }));

            var contextChunks = (await Task.WhenAll(mappingTasks)).ToList();

            _logger.LogInformation("TryRerankAsync completed: InputChunks={Input}, AfterScoreFilter={Output}, RerankTimeMs={Ms}",
                searchedChunks.Count, contextChunks.Count, ctx.RerankTimeMs);

            totalStopwatch.Stop();

            var metadata = ctx.ToMetadata();
            metadata.RerankScores = response.Results.ToDictionary(r => r.Index, r => r.Score);

            return new RagRetrievalResponse
            {
                Success = true,
                Query = request.Query,
                Chunks = contextChunks,
                TotalFound = totalFound,
                RerankingApplied = true,
                RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                Metadata = metadata,
                TargetMaterialIds = ctx.Intelligence.TargetMaterialIds   // ← add
            };
        }
        private async Task<RagRetrievalResponse> BuildNonRerankedResponse(
          RagRetrievalRequest request,
          List<SearchedChunk> searchedChunks,
          int totalFound,
          Stopwatch totalStopwatch,
          RetrievalContext ctx)
        {
            var mappingTasks = searchedChunks
                .Take(request.FinalTopK)
                .Select(async chunk => await MapToContextChunkAsync(chunk, new MappingRequest
                {
                    SourceTitle = chunk.Chunk.Material.Title,
                    MaterialType = chunk.Chunk.Material.Type,
                    relevanceScore = chunk.SimilarityScore
                }));

            var contextChunks = (await Task.WhenAll(mappingTasks)).ToList();

            totalStopwatch.Stop();

            return new RagRetrievalResponse
            {
                Success = true,
                Query = request.Query,
                Chunks = contextChunks,
                TotalFound = totalFound,
                RerankingApplied = false,
                RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                Metadata = ctx.ToMetadata()
            };
        }
    }
}
