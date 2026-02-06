using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.DocumentProcessing;
using AIEduPlatform.ML.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pgvector;
using System.Diagnostics;


namespace AIEduPlatform.ML.Services
{
    public class RAGService : IRAGService
    {
        private readonly IUnitOfWork _uow;
        private readonly IServiceProvider _serviceProvider;
        private readonly IContentChunker _chunker;
        private readonly IEmbeddingService _embeddingService;
        private readonly IRerankingService _rerankingService;
        private readonly IVisionService _visionService;
        private readonly ITranscriptionService _transcriptionService;
        private readonly ILogger<RAGService> _logger;

        private long _embeddingTimeMs;
        private long _searchTimeMs;
        private long _rerankTimeMs;

        private readonly SemaphoreSlim _documentSemaphore;
        private readonly SemaphoreSlim _pageSemaphore;
        private readonly SemaphoreSlim _transcriptionSemaphore;

        private readonly RagSettings _ragSettings;

        public RAGService(
            IUnitOfWork uow,
            IServiceProvider serviceProvider,
            IContentChunker chunker,
            IEmbeddingService embeddingService,
            IRerankingService rerankingService,
            IVisionService visionService,
            IOptions<RagSettings> options,
            ILogger<RAGService> logger,
            ITranscriptionService transcriptionService)
        {
            _uow = uow;
            _serviceProvider = serviceProvider;
            _chunker = chunker;
            _embeddingService = embeddingService;
            _rerankingService = rerankingService;
            _visionService = visionService;
            _transcriptionService = transcriptionService;
            _logger = logger;
            _ragSettings = options.Value;

            _documentSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentDocuments,
                _ragSettings.Concurrency.MaxConcurrentDocuments);

            _pageSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentPages,
                _ragSettings.Concurrency.MaxConcurrentPages);

            _transcriptionSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentTranscriptions,
                _ragSettings.Concurrency.MaxConcurrentTranscriptions);

            _logger.LogInformation(
                "RAGService initialized with settings: " +
                "MaxRetries={MaxRetries}, EmbeddingDelayMs={DelayMs}, " +
                "MaxDocs={MaxDocs}, MaxPages={MaxPages}, MaxTranscriptions={MaxTrans}, " +
                "MaxMaterials={MaxMats}",
                _ragSettings.MaxRetryAttempts,
                _ragSettings.EmbeddingDelayMs,
                _ragSettings.Concurrency.MaxConcurrentDocuments,
                _ragSettings.Concurrency.MaxConcurrentPages,
                _ragSettings.Concurrency.MaxConcurrentTranscriptions,
                _ragSettings.Concurrency.MaxConcurrentMaterials);
        }

        public ChunkingResult ChunkDocument(PageContent content, ChunkMetadata metadata, ChunkingOptions? options = null)
        {
            _logger.LogDebug("Chunking document: Source={Source}, Page={Page}, CustomOptions={HasOptions}",
                metadata.SourceTitle, content.PageNumber, options != null);

            if (options != null)
                _chunker.ResizeChunk(options);

            var result = new ChunkingResult { Chunks = _chunker.ChunkPageContent(content, metadata) };

            _logger.LogDebug("Chunking complete: Source={Source}, Page={Page}, ChunksProduced={Count}",
                metadata.SourceTitle, content.PageNumber, result.Chunks.Count);

            return result;
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

        public async Task<int> GetChunkCountAsync(Guid materialId, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("Fetching chunk count: MaterialId={MaterialId}", materialId);

            var fetched = await _uow.Materials.GetMaterialByIdAsync(materialId, includeChunks: true, cancellationToken);
            var count = fetched?.Chunks?.Count ?? 0;

            _logger.LogDebug("Chunk count retrieved: MaterialId={MaterialId}, Count={Count}", materialId, count);

            return count;
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

        public async Task<RagIndexResponse> IndexAsync(RagIndexRequest request, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("IndexAsync started: CourseId={CourseId}, Reindex={Reindex}", request.CourseId, request.Reindex);

            var courseExists = await _uow.Courses.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
            {
                _logger.LogWarning("IndexAsync failed: course not found. CourseId={CourseId}", request.CourseId);
                throw new KeyNotFoundException("Course not found");
            }

            var totalMaterialsCount = await _uow.Materials.GetMaterialsCountAsync(request.CourseId, cancellationToken);
            if (totalMaterialsCount == 0)
            {
                _logger.LogWarning("IndexAsync failed: course has no materials. CourseId={CourseId}", request.CourseId);
                throw new InvalidOperationException($"Course {request.CourseId} has no materials");
            }

            if (!request.Reindex)
            {
                var hasUnindexed = await _uow.Materials.HasUnindexedMaterialsAsync(request.CourseId, cancellationToken);
                if (!hasUnindexed)
                {
                    _logger.LogInformation("IndexAsync skipped: no unindexed materials. CourseId={CourseId}", request.CourseId);

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

            _logger.LogInformation("IndexAsync: materials to index identified. CourseId={CourseId}, MaterialCount={Count}",
                request.CourseId, materialsToIndex.Count);

            int numOfChunksIndexed = 0;
            var totalStopwatch = System.Diagnostics.Stopwatch.StartNew();
            long totalEmbeddingMs = 0;
            int failedChunks = 0;

            var maxConcurrent = _ragSettings.Concurrency.MaxConcurrentMaterials;
            var semaphore = new SemaphoreSlim(maxConcurrent);

            var tasks = materialsToIndex.Select(async material =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    return await IndexMaterialByTypeAsync(request, material, course, numOfChunksIndexed, totalEmbeddingMs, failedChunks, cancellationToken);
                }
                finally
                {
                    semaphore.Release();
                }
            }).ToList();

            try
            {
                var results = await Task.WhenAll(tasks);
                numOfChunksIndexed = results.Sum(r => r.numOfChunksIndexed);
                totalEmbeddingMs = results.Sum(r => r.totalEmbeddingMs);
                failedChunks = results.Sum(r => r.failedChunks);
            }
            catch (Exception ex)
            {
                var failedMaterials = materialsToIndex
                    .Zip(tasks, (material, task) => new { material, task })
                    .Where(x => x.task.IsFaulted)
                    .Select(x => new { x.material.Id, x.material.Title, Error = x.task.Exception?.GetBaseException().Message })
                    .ToList();

                _logger.LogError("Failed materials: {@FailedMaterials}", failedMaterials);

                throw new AggregateException(
                    $"Failed to index {failedMaterials.Count} materials: {string.Join(", ", failedMaterials.Select(m => m.Title))}",
                    tasks.Where(t => t.IsFaulted).Select(t => t.Exception!).ToList());
            }

            totalStopwatch.Stop();

            _logger.LogInformation("IndexAsync completed: CourseId={CourseId}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, " +
                "FailureRatio={Ratio:P2}, TotalTimeMs={TotalMs}, EmbeddingTimeMs={EmbedMs}",
                request.CourseId, numOfChunksIndexed, failedChunks,
                numOfChunksIndexed > 0 ? (double)failedChunks / numOfChunksIndexed : 0,
                totalStopwatch.ElapsedMilliseconds, totalEmbeddingMs);

            return new RagIndexResponse
            {
                Success = true,
                Error = string.Empty,
                ChunksIndexed = numOfChunksIndexed,
                CourseId = course.Id,
                IndexTimeMs = totalStopwatch.ElapsedMilliseconds,
                EmbeddingTimeMs = totalEmbeddingMs,
                ChunksFailed = failedChunks,
                FailureRatio = numOfChunksIndexed > 0 ? (double)failedChunks / numOfChunksIndexed : 0,
            };
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

        public async Task<RagRetrievalResponse> RetrieveAsync(RagRetrievalRequest request, CancellationToken ct = default)
        {
            _logger.LogInformation("RetrieveAsync started: CourseId={CourseId}, LectureIds={LectureIds}, MaterialIds={MaterialIds}, TopK={TopK}, FinalTopK={FinalTopK}, UseReranking={Rerank}",
                request.CourseId,
                request.LectureIds != null ? string.Join(",", request.LectureIds) : "null",
                request.MaterialIds != null ? string.Join(",", request.MaterialIds) : "null",
                request.TopK, request.FinalTopK, request.UseReranking);

            _embeddingTimeMs = 0;
            _searchTimeMs = 0;
            _rerankTimeMs = 0;

            var totalStopwatch = Stopwatch.StartNew();

            await ValidateRequestAsync(request, ct);
            await EnsureMaterialsIndexedAsync(request, ct);

            var materials = await LoadMaterialsAsync(request, ct);

            _logger.LogDebug("RetrieveAsync: loaded {Count} material(s) for search. CourseId={CourseId}",
                materials.Count, request.CourseId);

            var embeddedQuery = await EmbedQueryWithRetryAsync(request.Query, ct);

            var searchedChunks = await SearchChunksAsync(materials, embeddedQuery, request.TopK, ct);
            var totalFound = searchedChunks.Count;

            _logger.LogInformation("RetrieveAsync: vector search returned {TotalFound} chunk(s). CourseId={CourseId}, SearchTimeMs={SearchMs}",
                totalFound, request.CourseId, _searchTimeMs);

            var result = request.UseReranking
                ? await TryRerankAsync(request, searchedChunks, totalStopwatch, totalFound, ct)
                : null;

            if (result != null)
            {
                _logger.LogInformation("RetrieveAsync completed (reranked): CourseId={CourseId}, TotalFound={TotalFound}, FinalChunks={Final}, " +
                    "EmbeddingTimeMs={EmbedMs}, SearchTimeMs={SearchMs}, RerankTimeMs={RerankMs}, TotalTimeMs={TotalMs}",
                    request.CourseId, totalFound, result.Chunks.Count,
                    _embeddingTimeMs, _searchTimeMs, _rerankTimeMs, result.RetrievalTimeMs);
                return result;
            }

            var nonRerankedResult = BuildNonRerankedResponse(request, searchedChunks, totalFound, totalStopwatch);

            _logger.LogInformation("RetrieveAsync completed (no reranking): CourseId={CourseId}, TotalFound={TotalFound}, FinalChunks={Final}, " +
                "EmbeddingTimeMs={EmbedMs}, SearchTimeMs={SearchMs}, TotalTimeMs={TotalMs}",
                request.CourseId, totalFound, nonRerankedResult.Chunks.Count,
                _embeddingTimeMs, _searchTimeMs, nonRerankedResult.RetrievalTimeMs);

            return nonRerankedResult;
        }


        private Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexMaterialByTypeAsync(RagIndexRequest request, Material material, Course? course, int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks, CancellationToken cancellationToken)
        {
            return material.Type switch
            {
                Core.Domain.Enums.MaterialType.Document =>
                    IndexDocumentAsync(course, material, request.options, cancellationToken),

                Core.Domain.Enums.MaterialType.Audio =>
                    IndexAudioAsync(course, material, request.options, cancellationToken),

                _ => Task.FromResult((numOfChunksIndexed, totalEmbeddingMs, failedChunks))
            };
        }
        private async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexAudioAsync(
            Course course,
            Material material,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexAudioAsync started: MaterialId={MaterialId}, Title={Title}",
                material.Id, material.Title);

            await _documentSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

            try
            {
                int numOfChunks = 0;
                long totalEmbeddingMs = 0;
                int failedChunks = 0;
                var metadata = CreateChunkMetadata(course, material);

                if (options != null)
                {
                    _chunker.ResizeChunk(options);
                }

                List<AudioChunk> audioChunks;
                using (var audioExtractor = new AudioContentExtractor(material.FileUrl))
                {
                    audioChunks = await audioExtractor.ExtractChunksAsync(
                        chunkDurationSeconds: _ragSettings.AudioProcessing.ChunkDurationSeconds,
                        cancellationToken: cancellationToken);

                    if (audioChunks == null || !audioChunks.Any())
                    {
                        _logger.LogWarning("IndexAudioAsync: no audio chunks extracted. MaterialId={MaterialId}, Title={Title}",
                            material.Id, material.Title);
                        throw new InvalidOperationException("No audio chunks extracted from audio file");
                    }
                }

                _logger.LogInformation("IndexAudioAsync: audio chunked into {ChunkCount} segments. MaterialId={MaterialId}, TotalDuration={Duration}s",
                    audioChunks.Count, material.Id, audioChunks.Sum(c => c.DurationSeconds));

                var allMaterialChunks = new List<MaterialChunk>();
                var batchSize = _ragSettings.AudioProcessing.TranscriptionBatchSize;
                int totalBatches = (audioChunks.Count + batchSize - 1) / batchSize;

                for (int i = 0; i < audioChunks.Count; i += batchSize)
                {
                    var batch = audioChunks.Skip(i).Take(batchSize).ToList();
                    int currentBatch = (i / batchSize) + 1;

                    _logger.LogDebug("IndexAudioAsync: processing batch {BatchNumber}/{TotalBatches} with {ChunkCount} chunks",
                        currentBatch, totalBatches, batch.Count);

                    var transcriptionTasks = batch.Select(async chunk =>
                    {
                        try
                        {
                            return await TranscribeAudioChunkAsync(chunk, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to transcribe chunk {Index}", chunk.Index);
                            return null;
                        }
                    });

                    TranscriptionResult[] transcriptionResults;
                    try
                    {
                        transcriptionResults = (await Task.WhenAll(transcriptionTasks))
                                .Where(r => r != null)
                                .ToArray();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IndexAudioAsync: batch transcription failed for batch {BatchNumber}", currentBatch);
                        failedChunks += batch.Count;
                        continue;
                    }

                    // Process transcriptions
                    for (int j = 0; j < transcriptionResults.Length; j++)
                    {
                        var transcriptionResult = transcriptionResults[j];
                        var audioChunk = batch[j];

                        try
                        {
                            var textChunks = _chunker.ChunkTranscribedAudio(
                                transcribedText: transcriptionResult.TransResult.Text,
                                segments: transcriptionResult.TransResult.Segments,
                                baseMetadata: metadata,
                                audioChunkIndex: audioChunk.Index,
                                audioStartTime: audioChunk.StartTimeSeconds,
                                audioEndTime: audioChunk.StartTimeSeconds + audioChunk.DurationSeconds
                            );

                            _logger.LogDebug("IndexAudioAsync: created {ChunkCount} text chunks from audio chunk {Index}",
                                textChunks.Count, audioChunk.Index);

                            var embedResults = await EmbedChunksBatchAsync(
                                textChunks,
                                material.Id,  
                                cancellationToken);

                            foreach (var result in embedResults)
                            {
                                if (result.success && result.materialChunk != null)
                                {
                                    allMaterialChunks.Add(result.materialChunk);
                                    numOfChunks++;
                                    totalEmbeddingMs += result.embeddingTimeMs;
                                }
                                else
                                {
                                    failedChunks++;
                                }
                            }

                            _logger.LogDebug("IndexAudioAsync: embedded {SuccessCount} chunks, {FailedCount} failed for audio chunk {Index}",
                                embedResults.Count(r => r.success), embedResults.Count(r => !r.success), audioChunk.Index);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "IndexAudioAsync: failed to process transcription for audio chunk {Index}", audioChunk.Index);
                            failedChunks++;
                        }
                    }

                    _logger.LogInformation("IndexAudioAsync: completed batch {BatchNumber}/{TotalBatches}. Total chunks so far: {ChunkCount}",
                        currentBatch, totalBatches, numOfChunks);
                }

                if (allMaterialChunks.Any())
                {
                    _logger.LogDebug("IndexAudioAsync: saving {ChunkCount} chunks to database", allMaterialChunks.Count);

                    using var scope = _serviceProvider.CreateScope();
                    var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                    await scopedUow.BeginTransactionAsync(cancellationToken);

                    try
                    {
                        await scopedUow.Materials.AddRangeOfMaterialChunksAsync(
                            allMaterialChunks,
                            material.Id,
                            cancellationToken);

                        await scopedUow.CommitTransactionAsync(cancellationToken);

                        _logger.LogInformation("IndexAudioAsync completed successfully: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}, TotalAudioDuration={Duration}s",
                            material.Id, material.Title, numOfChunks, failedChunks, totalEmbeddingMs, audioChunks.Sum(c => c.DurationSeconds));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "IndexAudioAsync: database transaction failed");
                        await scopedUow.RollbackTransactionAsync(cancellationToken);
                        throw;
                    }
                }
                else
                {
                    _logger.LogWarning("IndexAudioAsync: no chunks produced after processing all audio segments. MaterialId={MaterialId}, Title={Title}",
                        material.Id, material.Title);
                }

                return (numOfChunks, totalEmbeddingMs, failedChunks);
            }
            finally
            {
                _documentSemaphore.Release();
            }
        }
        private async Task<TranscriptionResult> TranscribeAudioChunkAsync(
            AudioChunk audioChunk,
            CancellationToken cancellationToken)
        {
            _logger.LogDebug("TranscribeAudioChunkAsync: transcribing chunk {Index}, Duration={Duration}s, StartTime={Start}s",
                audioChunk.Index, audioChunk.DurationSeconds, audioChunk.StartTimeSeconds);
            await _transcriptionSemaphore.WaitAsync(cancellationToken);
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                using var audioStream = new MemoryStream(audioChunk.AudioData);

                var result = await _transcriptionService.TranscribeFileAsync(
                    audioStream: audioStream,
                    fileName: $"chunk_{audioChunk.Index}.wav",
                    fileType: "wav",
                    sourceLanguage: null,
                    task: "transcribe",
                    includeTimestamps: true,
                    includeMetadata: true,
                    ct: cancellationToken);

                stopwatch.Stop();

                _logger.LogDebug("TranscribeAudioChunkAsync completed: ChunkIndex={Index}, TextLength={Length}, Language={Language}, ProcessingTime={TimeMs}ms",
                    audioChunk.Index, result.Text?.Length ?? 0, result.Language, stopwatch.ElapsedMilliseconds);

                return new TranscriptionResult
                {
                    AudioChunkIndex = audioChunk.Index,
                    TransResult = result,
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TranscribeAudioChunkAsync failed: ChunkIndex={Index}", audioChunk.Index);
                throw;
            }
            finally
            {
                _transcriptionSemaphore.Release();
            }
        }
        private async Task<List<(bool success, MaterialChunk? materialChunk, long embeddingTimeMs)>> EmbedChunksBatchAsync(
            List<ContextChunk> contextChunks,
            Guid materialId,
            CancellationToken cancellationToken)
        {
            if (contextChunks == null || !contextChunks.Any())
            {
                return new List<(bool success, MaterialChunk? materialChunk, long embeddingTimeMs)>();
            }

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var batchRequest = new BatchEmbeddingRequest
                {
                    Texts = contextChunks.Select((chunk, idx) => new EmbeddingChunk
                    {
                        Index = idx,
                        Text = chunk.Content
                    }).ToList(),
                    Normalize = true,
                    ContinueOnError = true 
                };

                _logger.LogDebug("EmbedChunksBatchAsync: requesting embeddings for {Count} chunks", contextChunks.Count);

                var batchResult = await _embeddingService.GetBatchEmbeddingAsync(batchRequest, cancellationToken);

                stopwatch.Stop();

                if (batchResult?.Results == null)
                {
                    _logger.LogWarning("EmbedChunksBatchAsync: batch embedding returned no results");
                    return contextChunks.Select(_ => (false, (MaterialChunk?)null, 0L)).ToList();
                }

                _logger.LogInformation("EmbedChunksBatchAsync completed: Successful={Successful}, Failed={Failed}, TimeMs={TimeMs}",
                    batchResult.Successful, batchResult.Failed, stopwatch.ElapsedMilliseconds);

                var results = new List<(bool success, MaterialChunk? materialChunk, long embeddingTimeMs)>();

                foreach (var embeddingResult in batchResult.Results.OrderBy(r => r.Index))
                {
                    if (embeddingResult.Index >= contextChunks.Count)
                    {
                        _logger.LogWarning("EmbedChunksBatchAsync: result index {Index} out of range", embeddingResult.Index);
                        results.Add((false, null, 0));
                        continue;
                    }

                    var contextChunk = contextChunks[embeddingResult.Index];

                    if (!embeddingResult.Success || embeddingResult.Embedding == null)
                    {
                        _logger.LogWarning("EmbedChunksBatchAsync: chunk {Index} failed - {Error}",
                            embeddingResult.Index, embeddingResult.Error);
                        results.Add((false, null, 0));
                        continue;
                    }

                    var materialChunk = new MaterialChunk
                    {
                        MaterialId = materialId,
                        Content = contextChunk.Content,
                        Embedding = new Vector(embeddingResult.Embedding.ToArray()),
                        Section = contextChunk.Metadata.Section,
                        LectureName = contextChunk.Metadata.LectureName,
                        CourseName = contextChunk.Metadata.CourseName,
                        PageOrTimestamp = contextChunk.Metadata.PageOrTimestamp
                    };

                    var timePerChunk = stopwatch.ElapsedMilliseconds / batchResult.Successful;

                    results.Add((true, materialChunk, timePerChunk));
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EmbedChunksBatchAsync failed");
                return contextChunks.Select(_ => (false, (MaterialChunk?)null, 0L)).ToList();
            }
        }
        private async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexDocumentAsync(Course course, Material material, ChunkingOptions? options = null, CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexDocumentAsync started: MaterialId={MaterialId}, Title={Title}",
                material.Id, material.Title);

            await _documentSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                int numOfChunks = 0;
                long totalEmbeddingMs = 0;
                int failedChunks = 0;

                if (options != null)
                {
                    _chunker.ResizeChunk(options);
                }

                var metadata = CreateChunkMetadata(course, material);

                using (var pdfReader = new PdfContentExtractor(material.FileUrl, _visionService))
                {
                    var pages = await pdfReader.ExtractAllPagesAsync(cancellationToken);

                    if (pages == null || !pages.Any())
                    {
                        _logger.LogWarning("IndexDocumentAsync: no pages extracted from material. MaterialId={MaterialId}, Title={Title}",
                            material.Id, material.Title);
                        throw new InvalidOperationException("No pages extracted");
                    }

                    _logger.LogDebug("IndexDocumentAsync: pages extracted. MaterialId={MaterialId}, PageCount={Pages}",
                        material.Id, pages.Count);

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
                    {

                        using var scope = _serviceProvider.CreateScope();  
                        var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>(); 

                        await scopedUow.BeginTransactionAsync(cancellationToken);
                        try
                        {
                            await scopedUow.Materials.AddRangeOfMaterialChunksAsync(allchunks, material.Id, cancellationToken);
                            await scopedUow.CommitTransactionAsync(cancellationToken);
                        }
                        catch(Exception ex)
                        {
                            _logger.LogError(ex, "IndexAudioAsync: database transaction failed");
                            await scopedUow.RollbackTransactionAsync(cancellationToken);
                            throw;
                        }
                        _logger.LogInformation("IndexDocumentAsync completed: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}",
                            material.Id, material.Title, numOfChunks, failedChunks, totalEmbeddingMs);
                    }
                    else
                    {
                        _logger.LogWarning("IndexDocumentAsync: no chunks produced after processing all pages. MaterialId={MaterialId}, Title={Title}",
                            material.Id, material.Title);
                    }
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
            _logger.LogDebug("ProcessPageAsync started: MaterialId={MaterialId}, Page={Page}",
                metadata.MaterialId, page.PageNumber);

            await _pageSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var pageChunks = ChunkDocument(page, metadata, options);

                if (!pageChunks.Chunks.Any())
                {
                    _logger.LogWarning("ProcessPageAsync: no chunks produced for page. MaterialId={MaterialId}, Page={Page}",
                        metadata.MaterialId, page.PageNumber);
                    throw new Exception("Failed to fetch page chunks");
                }

                _logger.LogDebug("ProcessPageAsync: sending {ChunkCount} chunks for embedding. MaterialId={MaterialId}, Page={Page}",
                    pageChunks.Chunks.Count, metadata.MaterialId, page.PageNumber);

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

                    if (response.Failed > 0)
                        _logger.LogWarning("ProcessPageAsync: initial batch embedding had failures, entering retry. MaterialId={MaterialId}, Page={Page}, Failed={Failed}",
                            metadata.MaterialId, page.PageNumber, response.Failed);

                    response = await RetryFailedEmbeddingsAsync(pageChunks.Chunks, response, cancellationToken);
                }
                finally
                {
                    embedWatch.Stop();
                }

                if (pageChunks.Chunks.Count != response.Results.Count)
                {
                    _logger.LogError("ProcessPageAsync: chunk/embedding count mismatch. MaterialId={MaterialId}, Page={Page}, Chunks={Chunks}, Embeddings={Embeddings}",
                        metadata.MaterialId, page.PageNumber, pageChunks.Chunks.Count, response.Results.Count);
                    throw new InvalidOperationException(
                        $"Page {page.PageNumber} of material {metadata.SourceTitle} has mismatched chunks and embeddings.");
                }

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

                _logger.LogDebug("ProcessPageAsync completed: MaterialId={MaterialId}, Page={Page}, ChunksProduced={Produced}, Failed={Failed}, EmbeddingTimeMs={EmbedMs}",
                    metadata.MaterialId, page.PageNumber, materialChunks.Count, response.Failed, embedWatch.ElapsedMilliseconds);

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

            _logger.LogWarning("RetryFailedEmbeddingsAsync: starting retry loop. Failed={Failed}, TotalChunks={Total}, MaxAttempts={Max}",
                response.Failed, totalChunks, _ragSettings.MaxRetryAttempts);

            while (response.Failed > 0 && numOfTries < _ragSettings.MaxRetryAttempts)
            {
                numOfTries++;

                var failedChunksIndexes = new HashSet<int>(response.ErrorsSummary.Select(e => e.Index));

                var failedChunksWithOriginalIndex = pageChunks
                    .Select((chunk, index) => new { chunk, originalIndex = index })
                    .Where(x => failedChunksIndexes.Contains(x.originalIndex))
                    .ToList();

                if (!failedChunksWithOriginalIndex.Any())
                {
                    _logger.LogDebug("RetryFailedEmbeddingsAsync: no matching failed chunks found in source list, breaking. Attempt={Attempt}",
                        numOfTries);
                    break;
                }

                _logger.LogInformation("RetryFailedEmbeddingsAsync: attempt {Attempt}/{Max}, retrying {Count} chunk(s).",
                    numOfTries, _ragSettings.MaxRetryAttempts, failedChunksWithOriginalIndex.Count);

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

                _logger.LogInformation("RetryFailedEmbeddingsAsync: attempt {Attempt}/{Max} result. Successful={Successful}, StillFailed={Failed}",
                    numOfTries, _ragSettings.MaxRetryAttempts, response.Successful, response.Failed);

                await Task.Delay(_ragSettings.EmbeddingDelayMs, cancellationToken);
            }

            double lossRatio = (double)response.Failed / totalChunks;
            if (lossRatio > _ragSettings.MaxAcceptableFailureRatio)
            {
                _logger.LogError("RetryFailedEmbeddingsAsync: exceeded max acceptable failure ratio. Failed={Failed}, Total={Total}, LossRatio={Ratio:P2}, Threshold={Threshold:P2}",
                    response.Failed, totalChunks, lossRatio, _ragSettings.MaxAcceptableFailureRatio);
                throw new Exception($"Failed to embed {response.Failed} chunks out of {totalChunks} ({lossRatio:P0}). Try again later.");
            }

            if (response.Failed == 0)
                _logger.LogInformation("RetryFailedEmbeddingsAsync: all chunks embedded successfully after {Attempts} attempt(s).", numOfTries);

            return response;
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
        private async Task<EmbeddingResponse> EmbedQueryWithRetryAsync(string query, CancellationToken ct)
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
                        _embeddingTimeMs = stopwatch.ElapsedMilliseconds;

                        _logger.LogDebug("EmbedQueryWithRetryAsync: query embedded successfully on attempt {Attempt}. EmbeddingTimeMs={Ms}",
                            attempt, _embeddingTimeMs);

                        return response;
                    }

                    _logger.LogWarning("EmbedQueryWithRetryAsync: embedding service returned null on attempt {Attempt}/{Max}.",
                        attempt, _ragSettings.MaxRetryAttempts);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "EmbedQueryWithRetryAsync: failed on attempt {Attempt}/{Max}.",
                        attempt, _ragSettings.MaxRetryAttempts);
                }

                await Task.Delay(_ragSettings.EmbeddingDelayMs, ct);
            }

            _logger.LogError("EmbedQueryWithRetryAsync: exhausted all {Max} retry attempts for query embedding.", _ragSettings.MaxRetryAttempts);
            throw new Exception($"Failed to embed user query after {_ragSettings.MaxRetryAttempts} attempts.");
        }
        private async Task<List<MaterialChunk>> SearchChunksAsync(
            List<Material> materials,
            EmbeddingResponse embeddedQuery,
            int topK,
            CancellationToken ct)
        {
            _logger.LogDebug("SearchChunksAsync: starting vector search across {MaterialCount} material(s), TopK={TopK}",
                materials.Count, topK);

            var stopwatch = Stopwatch.StartNew();
            var queryVector = new Vector(embeddedQuery.Embedding.ToArray());
            var searchedChunks = new List<MaterialChunk>();

            foreach (var material in materials)
            {
                var result = await _uow.Materials.SearchChunksByMaterialAsync(
                    material.Id,
                    queryVector,
                    topK,
                    ct);

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
            _searchTimeMs = stopwatch.ElapsedMilliseconds;

            _logger.LogDebug("SearchChunksAsync completed: TotalChunksFound={Total}, SearchTimeMs={Ms}",
                searchedChunks.Count, _searchTimeMs);

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
            {
                _logger.LogDebug("TryRerankAsync: no chunks to rerank, skipping.");
                return null;
            }

            _logger.LogInformation("TryRerankAsync: starting reranking. InputChunks={Input}, FinalTopK={FinalTopK}, MinScore={MinScore}",
                searchedChunks.Count, request.FinalTopK, request.MinScore);

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
            {
                _logger.LogWarning("TryRerankAsync: reranking service returned null results.");
                return null;
            }

            var contextChunks = response.Results
                .Where(r => r.Score >= request.MinScore)
                .Where(r => r.Index >= 0 && r.Index < searchedChunks.Count)
                .Select(r => MapToContextChunk(searchedChunks[r.Index], r.Score))
                .ToList();

            _logger.LogInformation("TryRerankAsync completed: InputChunks={Input}, AfterScoreFilter={Output}, RerankTimeMs={Ms}",
                searchedChunks.Count, contextChunks.Count, _rerankTimeMs);

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
                    CourseName = chunk?.CourseName ?? "Unknown",
                    LectureName = chunk?.LectureName ?? "Unknown",
                    Section = chunk?.Section ?? "Unknown",
                    PageOrTimestamp = chunk?.PageOrTimestamp ?? "Unknown",
                    MaterialId = chunk.MaterialId,
                }
            };
        }
    }
}