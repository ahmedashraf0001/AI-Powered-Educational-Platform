using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Infrastructure.Data;
using AIEduPlatform.ML.DocumentProcessing;
using Microsoft.EntityFrameworkCore;
using Pgvector;


namespace AIEduPlatform.ML.Services
{
    public class RAGService : IRAGService
    {
        // there is an issue with how optimized course/lec/mat retrieval queries here that needs to be refined.
        // some queries causes full inclusion and retrieval to elements in memeory then filtered

        private readonly IContentChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IRerankingService _rerankingService;
        private readonly ICourseRepository _courseRepository;
        private readonly ILectureRepository _lectureRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IVisionService _visionService;
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        private const int MAX_RETRY_ATTEMPTS = 5;
        private const double MAX_ACCEPTABLE_FAILURE_RATIO = 0.3;

        public RAGService(IContentChunker chunker, IEmbeddingService embeddingService, IRerankingService rerankingService, ICourseRepository courseRepository, ILectureRepository lectureRepository, IMaterialRepository materialRepository, IVisionService visionService, IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _chunker = chunker;
            _embeddingService = embeddingService;
            _rerankingService = rerankingService;
            _courseRepository = courseRepository;
            _lectureRepository = lectureRepository;
            _materialRepository = materialRepository;
            _visionService = visionService;
            _dbContextFactory = dbContextFactory;
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
            int numOfChunks = 0;
            long totalEmbeddingMs = 0;
            int failedChunks = 0;
            var metadata = CreateChunkMetadata(course, material);

            using (var pdfReader = new PdfContentExtractor(material.FileUrl, _visionService))
            {
                var pages = await pdfReader.ExtractAllPagesAsync(cancellationToken);
                var allchunks = new List<MaterialChunk>();
                foreach (var page in pages)
                {
                    var result = await ProcessPageAsync(page, metadata, options, cancellationToken);
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

        private async Task<(List<MaterialChunk> materialChunks, long EmbeddingTimeMs, int failedChunksCount)> ProcessPageAsync(PageContent page, ChunkMetadata metadata, ChunkingOptions? options = null, CancellationToken cancellationToken = default)
        {
            var pageChunks = ChunkDocument(page, metadata);

            if (!pageChunks.Chunks.Any())
                throw new Exception("Failed To Fetch Page Chunks");

            var embedWatch = System.Diagnostics.Stopwatch.StartNew();


            BatchEmbeddingResponse response;
            BatchEmbeddingResponse retry;
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
                retry = await RetryFailedEmbeddingsAsync(pageChunks.Chunks, response, cancellationToken);

            }
            finally
            {
                embedWatch.Stop();
            }

            if (pageChunks.Chunks.Count != response.Results.Count)
                throw new InvalidOperationException($"Page {page.PageNumber} of material {metadata.SourceTitle} has mismatched chunks and embeddings.");

            var materialChunks = pageChunks.Chunks
                .Zip(retry.Results, (chunk, embedding) => (chunk, embedding))
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

            return (materialChunks, embedWatch.ElapsedMilliseconds, retry.Failed);
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

            while (response.Failed > 0 && numOfTries < MAX_RETRY_ATTEMPTS)
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
            }

            double lossRatio = (double)response.Failed / totalChunks;
            if (lossRatio > MAX_ACCEPTABLE_FAILURE_RATIO)
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

        public async Task<RagRetrievalResponse> RetrieveAsync(RagRetrievalRequest request, CancellationToken cancellationToken = default)
        {
            var courseExists = await _courseRepository.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
                throw new KeyNotFoundException("Course not found");

            var hasLectures = await _lectureRepository.CourseHasLecturesAsync(request.CourseId, cancellationToken);
            if (!hasLectures)
                throw new InvalidOperationException($"Course {request.CourseId} has no lectures");

            if (request.LectureIds != null && request.LectureIds.Any())
            {
                var lecturesExist = await _lectureRepository.LecturesExistInCourseAsync(
                    request.CourseId,
                    request.LectureIds,
                    cancellationToken);

                if (!lecturesExist)
                    throw new InvalidOperationException($"One or more specified lectures are not found in Course {request.CourseId}");
            }

            var hasUnindexedMaterials = await _materialRepository.HasUnindexedMaterialsInScopeAsync(
                request.CourseId,
                request.LectureIds,
                request.MaterialIds,
                cancellationToken);

            if (hasUnindexedMaterials)
            {
                await IndexAsync(new RagIndexRequest
                {
                    CourseId = request.CourseId,
                }, cancellationToken);
            }
            var materials = await _materialRepository.GetMaterialsForRetrievalAsync(
                 request.CourseId,
                 request.LectureIds,
                 request.MaterialIds,
                 cancellationToken);

            if (!materials.Any())
                throw new InvalidOperationException($"No materials found matching the specified criteria in Course {request.CourseId}");

            // to be continued kosom da mshro3

        }

        public Task<List<ContextChunk>> RetrieveContextAsync(string query, Guid courseId, int topK = 5, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RagRetrievalResponse> RetrieveForCourseAsync(string query, Guid courseId, int topK = 5, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<RagRetrievalResponse> RetrieveForLecturesAsync(string query, IEnumerable<Guid> lectureIds, int topK = 5, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
