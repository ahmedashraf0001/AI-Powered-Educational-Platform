using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pgvector;
using System.Diagnostics;

namespace AIEduPlatform.ML.Services.RAG
{
    public abstract class MaterialIndexingHelperBase
    {
        protected readonly IEmbeddingService _embeddingService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly RagSettings _ragSettings;
        protected readonly ILogger _logger;
        protected readonly SemaphoreSlim _embeddingSemaphore;

        protected MaterialIndexingHelperBase(
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            RagSettings ragSettings,
            ILogger logger)
        {
            _embeddingService = embeddingService;
            _serviceProvider = serviceProvider;
            _ragSettings = ragSettings;
            _logger = logger;

            _embeddingSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentEmbeddings,
                _ragSettings.Concurrency.MaxConcurrentEmbeddings);
        }

        protected static ChunkMetadata CreateChunkMetadata(Course course, Material material)
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

        protected async Task SaveMaterialChunksAsync(
            List<MaterialChunk> chunks,
            Material material,
            CancellationToken cancellationToken)
        {
            if (!chunks.Any())
            {
                _logger.LogWarning("SaveMaterialChunksAsync: no chunks to save. MaterialId={MaterialId}, Title={Title}",
                    material.Id, material.Title);
                return;
            }

            _logger.LogDebug("SaveMaterialChunksAsync: saving {ChunkCount} chunks to database. MaterialId={MaterialId}",
                chunks.Count, material.Id);

            using var scope = _serviceProvider.CreateScope();
            var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await scopedUow.BeginTransactionAsync(cancellationToken);

            try
            {
                await scopedUow.Materials.AddRangeOfMaterialChunksAsync(chunks, material.Id, cancellationToken);
                await scopedUow.CommitTransactionAsync(cancellationToken);

                _logger.LogDebug("SaveMaterialChunksAsync: successfully saved {ChunkCount} chunks. MaterialId={MaterialId}",
                    chunks.Count, material.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SaveMaterialChunksAsync: database transaction failed. MaterialId={MaterialId}", material.Id);
                await scopedUow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        protected async Task<(List<MaterialChunk> materialChunks, long EmbeddingTimeMs, int failedChunksCount)> EmbedChunksAsync(
            ChunkingResult chunks,
            ChunkMetadata metadata,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            await _embeddingSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var embedWatch = Stopwatch.StartNew();
                BatchEmbeddingResponse response;
                try
                {
                    response = await _embeddingService.GetBatchEmbeddingAsync(
                        new BatchEmbeddingRequest
                        {
                            Texts = chunks.Chunks.Select((chunk, index) => new EmbeddingChunk
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
                        _logger.LogWarning("EmbedChunksAsync: initial batch embedding had failures, entering retry. MaterialId={MaterialId}, Failed={Failed}",
                            metadata.MaterialId, response.Failed);

                    response = await RetryFailedEmbeddingsAsync(chunks.Chunks, response, cancellationToken);
                }
                finally
                {
                    embedWatch.Stop();
                }

                if (chunks.Chunks.Count != response.Results.Count)
                {
                    _logger.LogError("EmbedChunksAsync: chunk/embedding count mismatch. MaterialId={MaterialId}, Chunks={Chunks}, Embeddings={Embeddings}",
                        metadata.MaterialId, chunks.Chunks.Count, response.Results.Count);
                    throw new InvalidOperationException(
                        $"Material {metadata.MaterialId} ({metadata.SourceTitle}) has mismatched chunks ({chunks.Chunks.Count}) and embeddings ({response.Results.Count}).");
                }

                var materialChunks = chunks.Chunks
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
                        AdditionalData = x.chunk.AdditionalData,
                        PageOrTimestamp = x.chunk.Metadata.PageOrTimestamp
                    })
                    .ToList();

                _logger.LogDebug("EmbedChunksAsync completed: MaterialId={MaterialId}, ChunksProduced={Produced}, Failed={Failed}, EmbeddingTimeMs={EmbedMs}",
                    metadata.MaterialId, materialChunks.Count, response.Failed, embedWatch.ElapsedMilliseconds);

                return (materialChunks, embedWatch.ElapsedMilliseconds, response.Failed);
            }
            finally
            {
                _embeddingSemaphore.Release();
            }
        }

        protected async Task<BatchEmbeddingResponse> RetryFailedEmbeddingsAsync(
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
    }
}
