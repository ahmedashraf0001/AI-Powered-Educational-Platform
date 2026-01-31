using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Infrastructure.Data;
using AIEduPlatform.ML.DocumentProcessing;
using AIEduPlatform.ML.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pgvector;
using System.Diagnostics;


namespace AIEduPlatform.ML.Services
{
    public class RAGService : IRAGService
    {
        private readonly IContentChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IRerankingService _rerankingService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILectureRepository _lectureRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IVisionService _visionService;
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        private long _embeddingTimeMs;
        private long _searchTimeMs;
        private long _rerankTimeMs;

        private readonly SemaphoreSlim _documentSemaphore = new SemaphoreSlim(5, 5);
        private readonly SemaphoreSlim _pageSemaphore = new SemaphoreSlim(20, 20);

        private readonly RagSettings ragSettings;

        public RAGService(IContentChunker chunker, IEmbeddingService embeddingService, IRerankingService rerankingService,
            ICourseRepository courseRepository, ILectureRepository lectureRepository, IMaterialRepository materialRepository,
            IVisionService visionService, IDbContextFactory<AppDbContext> dbContextFactory, IOptions<RagSettings> options)
        {
            _chunker = chunker;
            _embeddingService = embeddingService;
            _rerankingService = rerankingService;
            _courseRepository = courseRepository;
            _lectureRepository = lectureRepository;
            _materialRepository = materialRepository;
            _visionService = visionService;
            _dbContextFactory = dbContextFactory;
            ragSettings = options.Value;
        }

        public ChunkingResult ChunkDocument(PageContent content, ChunkMetadata metadata, ChunkingOptions? options = null)
        {
            if (options != null)
                _chunker.ResizeChunk(options);

            return new ChunkingResult { Chunks = _chunker.ChunkPageContent(content, metadata) };
        }

        public async Task<RagDeleteResponse> DeleteAsync(RagDeleteRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await (request switch
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

            }
            catch (Exception ex)
            {
                return new RagDeleteResponse
                {
                    Success = false,
                    Error = ex.Message,
                };
            }
        }

        public async Task<RagDeleteResponse> DeleteCourseAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deletedRows = await _courseRepository.DeleteByIdAsync(courseId, cancellationToken);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete course: {ex.Message}"
                };
            }
        }

        public async Task<RagDeleteResponse> DeleteLectureAsync(Guid lectureId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deletedRows = await _lectureRepository.DeleteByIdAsync(lectureId, cancellationToken);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete lecture: {ex.Message}"
                };
            }
        }

        public async Task<RagDeleteResponse> DeleteMaterialAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            try
            {
                var deletedRows = await _materialRepository.DeleteByIdAsync(materialId, cancellationToken);

                return new RagDeleteResponse
                {
                    Success = deletedRows > 0,
                    Error = deletedRows > 0 ? string.Empty : "No rows deleted"
                };
            }
            catch (Exception ex)
            {
                return new RagDeleteResponse
                {
                    Success = false,
                    Error = $"Failed to delete material: {ex.Message}"
                };
            }
        }

        public async Task<int> GetChunkCountAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            var fetched = await _materialRepository.GetMaterialByIdAsync(materialId, includeChunks: true, cancellationToken);
            return fetched?.Chunks?.Count ?? 0;
        }

        public async Task<RagIndexStats> GetIndexStatsAsync(Guid courseId, CancellationToken cancellationToken = default)
        {
            var course = await _courseRepository.GetCourseByIdAsync(courseId, new CourseIncludeOptions { IncludeMaterials = true }, cancellationToken);

            if (course == null)
                throw new KeyNotFoundException("Course not found");

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
            return result;
        }

        public async Task<RagIndexResponse> IndexAsync(RagIndexRequest request, CancellationToken cancellationToken = default)
        {
            var courseExists = await _courseRepository.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
                throw new KeyNotFoundException("Course not found");

            var totalMaterialsCount = await _materialRepository.GetMaterialsCountAsync(request.CourseId, cancellationToken);
            if (totalMaterialsCount == 0)
                throw new InvalidOperationException($"Course {request.CourseId} has no materials");

            if (!request.Reindex)
            {
                var hasUnindexed = await _materialRepository.HasUnindexedMaterialsAsync(request.CourseId, cancellationToken);
                if (!hasUnindexed)
                {
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

            var course = await _courseRepository.GetCourseByIdAsync(
                request.CourseId,
                new CourseIncludeOptions { IncludeMaterials = false },
                cancellationToken);

            var materialsToIndex = await _materialRepository.GetMaterialsToIndexAsync(
                request.CourseId,
                request.Reindex,
                cancellationToken);

            int numOfChunksIndexed = 0;
            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
            long totalEmbeddingMs = 0;
            int failedChunks = 0;
            var tasks = materialsToIndex.Select(material => IndexDocumentAsync(course, material, request.options, cancellationToken)).ToList();

            try
            {
                var results = await Task.WhenAll(tasks);
                numOfChunksIndexed = results.Sum(r => r.numOfChunksIndexed);
                totalEmbeddingMs = results.Sum(r => r.totalEmbeddingMs);
                failedChunks = results.Sum(r => r.failedChunks);
            }
            catch (Exception ex)
            {
                var exceptions = tasks
                    .Where(t => t.IsFaulted)
                    .Select(t => t.Exception?.InnerException?.Message ?? "Unknown error")
                    .ToList();

                throw new AggregateException($"Failed to index {exceptions.Count} materials",
                    tasks.Where(t => t.IsFaulted).Select(t => t.Exception!).ToList());
            }

            return new RagIndexResponse
            {
                Success = true,
                Error = string.Empty,
                ChunksIndexed = numOfChunksIndexed,
                CourseId = course.Id,
                IndexTimeMs= totalStopwatch.ElapsedMilliseconds,
                EmbeddingTimeMs = totalEmbeddingMs,
                ChunksFailed = failedChunks,
                FailureRatio = numOfChunksIndexed > 0 ? (double)failedChunks / numOfChunksIndexed : 0,
            };
        }

        private async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexDocumentAsync(Course course, Material material, ChunkingOptions? options = null, CancellationToken cancellationToken = default)
        {
            await _documentSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                int numOfChunks = 0;
                long totalEmbeddingMs = 0;
                int failedChunks = 0;
                var metadata = CreateChunkMetadata(course, material);

                using (var pdfReader = new PdfContentExtractor(material.FileUrl, _visionService))
                {
                    var pages = await pdfReader.ExtractAllPagesAsync(cancellationToken);

                    if (pages == null || !pages.Any())
                        throw new InvalidOperationException("No pages extracted");

                    var pageTasks = pages.Select(page => ProcessPageAsync(page, metadata, options, cancellationToken));
                    var pageResults = await Task.WhenAll(pageTasks);

                    var allchunks = new List<MaterialChunk>();
                    foreach (var result in pageResults)
                    {
                        allchunks.AddRange(result.materialChunks);
                        numOfChunks += result.materialChunks.Count;
                        failedChunks += result.failedChunksCount;
                        totalEmbeddingMs += result.EmbeddingTimeMs;
                    }
                    if (allchunks.Any())
                        await SaveChunksAsync(allchunks, material, cancellationToken);
                }
                return (numOfChunks, totalEmbeddingMs, failedChunks);

            }
            finally
            {
                _documentSemaphore.Release();
            }
        }

        private async Task<(List<MaterialChunk> materialChunks, long EmbeddingTimeMs, int failedChunksCount)> ProcessPageAsync(
            PageContent page,
            ChunkMetadata metadata,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            await _pageSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var pageChunks = ChunkDocument(page, metadata, options);

                if (!pageChunks.Chunks.Any())
                    throw new Exception("Failed to fetch page chunks");

                var embedWatch = Stopwatch.StartNew();
                BatchEmbeddingResponse response;

                try
                {
                    response = await _embeddingService.GetBatchEmbeddingAsync(
                        new BatchEmbeddingRequest
                        {
                            Texts = pageChunks.Chunks.Select((chunk, index) => new EmbeddingChunk
                            {
                                Index = index,
                                Text = chunk.Content
                            }).ToList(),
                            Normalize = true,
                            ContinueOnError = false
                        },
                        cancellationToken
                    );

                    response = await RetryFailedEmbeddingsAsync(pageChunks.Chunks, response, cancellationToken);
                }
                finally
                {
                    embedWatch.Stop();
                }

                if (pageChunks.Chunks.Count != response.Results.Count)
                    throw new InvalidOperationException(
                        $"Page {page.PageNumber} of material {metadata.SourceTitle} has mismatched chunks and embeddings.");

                var materialChunks = pageChunks.Chunks
                    .Zip(response.Results, (chunk, embedding) => (chunk, embedding))
                    .Where(x => x.embedding.Success)
                    .Select(x => new MaterialChunk
                    {
                        MaterialId = metadata.MaterialId,
                        Content = x.chunk.Content,
                        Embedding = new Vector(x.embedding.Embedding!.ToArray()),
                        Section = x.chunk.Metadata.Section,
                        LectureName = metadata.LectureName,
                        CourseName = metadata.CourseName,
                        PageOrTimestamp = x.chunk.Metadata.PageOrTimestamp
                    })
                    .ToList();

                return (materialChunks, embedWatch.ElapsedMilliseconds, response.Failed);
            }
            finally
            {
                _pageSemaphore.Release();
            }
        }

        private async Task<BatchEmbeddingResponse> RetryFailedEmbeddingsAsync(
            List<ContextChunk> pageChunks,
            BatchEmbeddingResponse response,
            CancellationToken cancellationToken)
        {
            if (response.Failed == 0)
                return response;

            int numOfTries = 0;
            int totalChunks = pageChunks.Count;

            while (response.Failed > 0 && numOfTries < ragSettings.MaxRetryAttempts)
            {
                numOfTries++;

                var failedChunksIndexes = new HashSet<int>(response.ErrorsSummary.Select(e => e.Index));

                var failedChunksWithOriginalIndex = pageChunks
                    .Select((chunk, index) => new { chunk, originalIndex = index })
                    .Where(x => failedChunksIndexes.Contains(x.originalIndex))
                    .ToList();

                if (!failedChunksWithOriginalIndex.Any())
                    break; 

                var retryResponse = await _embeddingService.GetBatchEmbeddingAsync(
                    new BatchEmbeddingRequest
                    {
                        Texts = failedChunksWithOriginalIndex
                            .Select(x => new EmbeddingChunk
                            {
                                Index = x.originalIndex,
                                Text = x.chunk.Content
                            })
                            .ToList(),
                        Normalize = true,
                        ContinueOnError = false
                    },
                    cancellationToken
                );

                var retryIndexes = new HashSet<int>(retryResponse.Results.Select(r => r.Index));
                response.Results = response.Results
                    .Where(r => !retryIndexes.Contains(r.Index))
                    .Concat(retryResponse.Results)
                    .OrderBy(r => r.Index)
                    .ToList();

                response.ErrorsSummary = retryResponse.ErrorsSummary;
                response.Successful = response.Results.Count(r => r.Success);
                response.Failed = response.ErrorsSummary.Count;

                await Task.Delay(ragSettings.EmbeddingDelayMs, cancellationToken);
            }

            double lossRatio = (double)response.Failed / totalChunks;
            if (lossRatio > ragSettings.MaxAcceptableFailureRatio)
            {
                throw new Exception($"Failed to embed {response.Failed} chunks out of {totalChunks} ({lossRatio:P0}). Try again later.");
            }
            return response;
        }

        private async Task SaveChunksAsync(List<MaterialChunk> materialChunks, Material material, CancellationToken cancellationToken)
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
            await dbContext.Chunks.AddRangeAsync(materialChunks, cancellationToken);
            dbContext.Attach(material);
            material.Indexed = true;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private static ChunkMetadata CreateChunkMetadata(Course course, Material material)
        {
            if (material.Lecture == null)
                throw new InvalidOperationException($"Material {material.Id} has no associated lecture");

            return new ChunkMetadata
            {
                SourceTitle = material.Title,
                MaterialType = material.Type.ToString(),
                LectureName = material.Lecture.Title,
                CourseName = course.Title,
                MaterialId = material.Id,
                LectureId = material.LectureId,
                CourseId = course.Id
            };
        }

        public async Task<bool> IsMaterialIndexedAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            var result = await _materialRepository.GetMaterialByIdAsync(materialId, includeChunks: false, cancellationToken);
            if (result == null)
                throw new KeyNotFoundException("Material Not Found");
            return result.Indexed;
        }

        public async Task<RagRetrievalResponse> RetrieveAsync(RagRetrievalRequest request, CancellationToken ct = default)
        {
            // Reset timing fields
            _embeddingTimeMs = 0;
            _searchTimeMs = 0;
            _rerankTimeMs = 0;

            var totalStopwatch = Stopwatch.StartNew();

            await ValidateRequestAsync(request, ct);
            await EnsureMaterialsIndexedAsync(request, ct);

            var materials = await LoadMaterialsAsync(request, ct);
            var embeddedQuery = await EmbedQueryWithRetryAsync(request.Query, ct);

            var searchedChunks = await SearchChunksAsync(materials, embeddedQuery, request.TopK, ct);
            var totalFound = searchedChunks.Count;

            var result = request.UseReranking
                ? await TryRerankAsync(request, searchedChunks, totalStopwatch, totalFound, ct)
                : null;

            if (result != null)
                return result;

            return BuildNonRerankedResponse(request, searchedChunks, totalFound, totalStopwatch);
        }

        private async Task ValidateRequestAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            if (!await _courseRepository.CourseExistsAsync(request.CourseId, ct))
                throw new KeyNotFoundException("Course not found");

            if (!await _lectureRepository.CourseHasLecturesAsync(request.CourseId, ct))
                throw new InvalidOperationException($"Course {request.CourseId} has no lectures");

            if (request.LectureIds?.Any() == true)
            {
                var lecturesExist = await _lectureRepository.LecturesExistInCourseAsync(
                    request.CourseId,
                    request.LectureIds,
                    ct);

                if (!lecturesExist)
                    throw new InvalidOperationException($"One or more specified lectures are not found in Course {request.CourseId}");
            }
        }

        private async Task EnsureMaterialsIndexedAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            var hasUnindexedMaterials = await _materialRepository.HasUnindexedMaterialsInScopeAsync(
                request.CourseId,
                request.LectureIds,
                request.MaterialIds,
                ct);

            if (hasUnindexedMaterials)
            {
                await IndexAsync(new RagIndexRequest
                {
                    CourseId = request.CourseId,
                }, ct);
            }
        }

        private async Task<List<Material>> LoadMaterialsAsync(RagRetrievalRequest request, CancellationToken ct)
        {
            var materials = await _materialRepository.GetMaterialsForRetrievalAsync(
                request.CourseId,
                request.LectureIds,
                request.MaterialIds,
                request.MaterialTypes,
                ct);

            if (!materials.Any())
                throw new InvalidOperationException($"No materials found matching the specified criteria in Course {request.CourseId}");

            return materials;
        }

        private async Task<EmbeddingResponse> EmbedQueryWithRetryAsync(string query, CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            int attempt = 0;

            while (attempt < ragSettings.MaxRetryAttempts)
            {
                attempt++;
                try
                {
                    var response = await _embeddingService.GetEmbeddingAsync(
                        new EmbeddingRequest { Text = query }, ct);

                    if (response != null)
                    {
                        stopwatch.Stop();
                        _embeddingTimeMs = stopwatch.ElapsedMilliseconds;
                        return response;
                    }
                }
                catch (Exception ex)
                {
                    //_logger.LogWarning(ex, "Failed to embed query on attempt {Attempt}/{MaxAttempts}",
                    //    attempt, MAX_RETRY_ATTEMPTS);
                }

                await Task.Delay(ragSettings.EmbeddingDelayMs, ct);
            }

            throw new Exception($"Failed to embed user query after {ragSettings.MaxRetryAttempts} attempts.");
        }

        private async Task<List<MaterialChunk>> SearchChunksAsync(
            List<Material> materials,
            EmbeddingResponse embeddedQuery,
            int topK,
            CancellationToken ct)
        {
            var stopwatch = Stopwatch.StartNew();
            var queryVector = new Vector(embeddedQuery.Embedding.ToArray());
            var searchedChunks = new List<MaterialChunk>();

            foreach (var material in materials)
            {
                var result = await _materialRepository.SearchChunksByMaterialAsync(
                    material.Id,
                    queryVector,
                    topK,
                    ct);

                if (result?.TopChunks?.Any() == true)
                    searchedChunks.AddRange(result.TopChunks);
            }

            stopwatch.Stop();
            _searchTimeMs = stopwatch.ElapsedMilliseconds;

            return searchedChunks;
        }

        private async Task<RagRetrievalResponse?> TryRerankAsync(
            RagRetrievalRequest request,
            List<MaterialChunk> searchedChunks,
            Stopwatch totalStopwatch,
            int totalFound,
            CancellationToken ct)
        {
            if (!searchedChunks.Any())
                return null;

            var rerankStopwatch = Stopwatch.StartNew();

            var rerankRequest = new RerankRequest
            {
                Query = request.Query,
                Chunks = searchedChunks.Select((chunk, index) => new RerankChunk
                {
                    Index = index,
                    Content = chunk.Content
                }).ToList(),
                TopK = request.FinalTopK,
                Return_Documents = true
            };

            var response = await _rerankingService.RerankAsync(rerankRequest, ct);
            rerankStopwatch.Stop();
            _rerankTimeMs = rerankStopwatch.ElapsedMilliseconds;

            if (response?.Results == null)
                return null;

            var contextChunks = response.Results
                .Where(r => r.Score >= request.MinScore)
                .Where(r => r.Index >= 0 && r.Index < searchedChunks.Count)
                .Select(r => MapToContextChunk(searchedChunks[r.Index], r.Score))
                .ToList();

            totalStopwatch.Stop();

            return new RagRetrievalResponse
            {
                Success = true,
                Query = request.Query,
                Chunks = contextChunks,
                TotalFound = totalFound,
                RerankingApplied = true,
                RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                Metadata = new RetrievalMetadata
                {
                    EmbeddingTimeMs = _embeddingTimeMs,
                    SearchTimeMs = _searchTimeMs,
                    RerankTimeMs = _rerankTimeMs,
                    RerankScores = response.Results.ToDictionary(r => r.Index, r => r.Score)
                }
            };
        }

        private RagRetrievalResponse BuildNonRerankedResponse(
            RagRetrievalRequest request,
            List<MaterialChunk> searchedChunks,
            int totalFound,
            Stopwatch totalStopwatch)
        {
            var contextChunks = searchedChunks
                .Take(request.FinalTopK)
                .Select(chunk => MapToContextChunk(chunk, 1.0f))
                .ToList();

            totalStopwatch.Stop();

            return new RagRetrievalResponse
            {
                Success = true,
                Query = request.Query,
                Chunks = contextChunks,
                TotalFound = totalFound,
                RerankingApplied = false,
                RetrievalTimeMs = totalStopwatch.ElapsedMilliseconds,
                Metadata = new RetrievalMetadata
                {
                    EmbeddingTimeMs = _embeddingTimeMs,
                    SearchTimeMs = _searchTimeMs,
                    RerankTimeMs = 0
                }
            };
        }

        private ContextChunk MapToContextChunk(MaterialChunk chunk, float relevanceScore)
        {
            return new ContextChunk
            {
                Content = chunk.Content,
                RelevanceScore = relevanceScore,
                Metadata = new ChunkMetadata
                {
                    SourceTitle = string.Empty,
                    MaterialType = string.Empty,
                    CourseName = chunk?.CourseName?? "Unknown",
                    LectureName = chunk?.LectureName?? "Unknown",
                    Section = chunk?.Section ?? "Unknown",
                    PageOrTimestamp = chunk?.PageOrTimestamp ?? "Unknown",
                    MaterialId = chunk.MaterialId,
                }
            };
        }
    }
}
