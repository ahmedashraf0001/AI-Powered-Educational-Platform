using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Services.Material_Processing;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pgvector;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AIEduPlatform.ML.Services.RAG
{
    public abstract class MaterialIndexingHelperBase
    {
        protected readonly IEmbeddingService _embeddingService;
        protected readonly IServiceProvider _serviceProvider;
        protected readonly RagSettings _ragSettings;
        protected readonly ILogger _logger;
        protected readonly SemaphoreSlim _embeddingSemaphore;
        protected readonly IConceptExtractionService _conceptExtractionService;
        private readonly SemaphoreSlim _conceptExtractionSemaphore;
        private readonly IOllamaServiceClient _summaryService;
        protected MaterialIndexingHelperBase(
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            IConceptExtractionService conceptExtractionService,
            RagSettings ragSettings,
            ILogger logger,
            IOllamaServiceClient summaryService)
        {
            _embeddingService = embeddingService;
            _serviceProvider = serviceProvider;
            _ragSettings = ragSettings;
            _logger = logger;
            _summaryService = summaryService;

            _embeddingSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentEmbeddings,
                _ragSettings.Concurrency.MaxConcurrentEmbeddings);

            _conceptExtractionService = conceptExtractionService;
            _conceptExtractionSemaphore = new SemaphoreSlim(
                ragSettings.Concurrency.MaxConcurrentConceptExtractions);
        }
        protected async Task<List<ChunkConceptsResult>> ExtractConceptsFromChunksAsync(
           List<MaterialChunk> savedChunks,
           CancellationToken ct)
        {
            var tasks = savedChunks.Select(async chunk =>
            {
                await _conceptExtractionSemaphore.WaitAsync(ct);
                try
                {
                    return await _conceptExtractionService.ExtractFromChunkAsync(
                        chunk.Content, chunk.Id, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Concept extraction failed for chunk {ChunkId}, continuing without it",
                        chunk.Id);
                    return new ChunkConceptsResult { ChunkId = chunk.Id };
                }
                finally
                {
                    _conceptExtractionSemaphore.Release();
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.ToList();
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

        protected async Task<List<MaterialChunk>> SaveMaterialChunksAsync(
            List<MaterialChunk> chunks,
            Material material,
            CancellationToken cancellationToken)
        {
            if (!chunks.Any())
            {
                _logger.LogWarning(
                    "SaveMaterialChunksAsync: no chunks to save. MaterialId={MaterialId}, Title={Title}",
                    material.Id, material.Title);
                return new List<MaterialChunk>();
            }

            _logger.LogDebug(
                "SaveMaterialChunksAsync: saving {ChunkCount} chunks. MaterialId={MaterialId}",
                chunks.Count, material.Id);

            using var scope = _serviceProvider.CreateScope();
            var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            await scopedUow.BeginTransactionAsync(cancellationToken);

            try
            {


                var summary = await _summaryService.GenerateSummaryAsync(
                    chunks.Take(5).Select(e => new ContextChunk
                    {
                        Content = e.Content,
                        Metadata = new ChunkMetadata
                        {
                            SourceTitle = material.Title,
                            MaterialType = material.Type.ToString(),
                            LectureName = material.Lecture?.Title,
                            CourseName = material.Lecture?.Course?.Title, 
                            MaterialId = material.Id,
                            LectureId = material.LectureId,
                            CourseId = material.Lecture?.CourseId ?? Guid.Empty
                        }
                    }).ToList(),
                    200,
                    true,
                    cancellationToken);
                material.Summary = summary.Content;
                material.Indexed = true;

                await scopedUow.Materials.UpdateAsync(material, cancellationToken);

                await scopedUow.Materials.AddRangeOfMaterialChunksAsync(
    chunks, material.Id, cancellationToken);

                await scopedUow.CommitTransactionAsync(cancellationToken);

                _logger.LogDebug(
                    "SaveMaterialChunksAsync: saved {ChunkCount} chunks. MaterialId={MaterialId}",
                    chunks.Count, material.Id);

                return chunks; // ← already fully constructed, just return them
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "SaveMaterialChunksAsync: transaction failed. MaterialId={MaterialId}", material.Id);
                await scopedUow.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Extracts semantic sections from material chunks via LLM and persists them.
        /// Call after SaveMaterialChunksAsync.
        /// </summary>
        protected async Task ExtractAndSaveSemanticSectionsAsync(
            List<MaterialChunk> savedChunks,
            Material material,
            CancellationToken cancellationToken)
        {
            try
            {
                bool isTimeBased = material.Type == MaterialType.Video || material.Type == MaterialType.Audio;

                // Build a combined transcript/content string from saved chunks
                var contentBuilder = new StringBuilder();
                foreach (var chunk in savedChunks.OrderBy(c => c.PageOrTimestamp))
                {
                    contentBuilder.AppendLine(chunk.Content);
                }

                var content = contentBuilder.ToString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("ExtractAndSaveSemanticSectionsAsync: no content to extract sections from. MaterialId={MaterialId}", material.Id);
                    return;
                }

                // Truncate if too long for LLM context (keep first ~32K chars)
                const int maxContentLength = 32000;
                if (content.Length > maxContentLength)
                {
                    content = content[..maxContentLength];
                    _logger.LogWarning("ExtractAndSaveSemanticSectionsAsync: content truncated to {MaxLength} chars for LLM. MaterialId={MaterialId}",
                        maxContentLength, material.Id);
                }

                var result = await _summaryService.ExtractSemanticSectionsAsync(content, isTimeBased, cancellationToken);

                if (result?.Sections == null || !result.Sections.Any())
                {
                    _logger.LogWarning("ExtractAndSaveSemanticSectionsAsync: LLM returned no sections. MaterialId={MaterialId}", material.Id);
                    return;
                }

                var semanticSections = new List<SemanticSection>();
                for (int i = 0; i < result.Sections.Count; i++)
                {
                    var s = result.Sections[i];
                    var section = new SemanticSection
                    {
                        MaterialId = material.Id,
                        Title = s.Title,
                        Summary = s.Summary,
                        OrderIndex = i
                    };

                    if (isTimeBased)
                    {
                        section.StartSeconds = ParseTimestampToSeconds(s.Start);
                        section.EndSeconds = ParseTimestampToSeconds(s.End);
                    }
                    else
                    {
                        section.StartPage = s.StartPage;
                        section.EndPage = s.EndPage;
                    }

                    semanticSections.Add(section);
                }

                using var scope = _serviceProvider.CreateScope();
                var scopedUow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                foreach (var section in semanticSections)
                {
                    await scopedUow.SemanticSections.AddAsync(section, cancellationToken);
                }
                await scopedUow.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("ExtractAndSaveSemanticSectionsAsync: saved {Count} sections. MaterialId={MaterialId}",
                    semanticSections.Count, material.Id);
            }
            catch (Exception ex)
            {
                // Non-fatal: section extraction failure should not block indexing
                _logger.LogError(ex, "ExtractAndSaveSemanticSectionsAsync failed (non-fatal). MaterialId={MaterialId}", material.Id);
            }
        }

        /// <summary>
        /// Parses "MM:SS" or "HH:MM:SS" timestamp to total seconds.
        /// </summary>
        private static int? ParseTimestampToSeconds(string? timestamp)
        {
            if (string.IsNullOrWhiteSpace(timestamp)) return null;

            var parts = timestamp.Split(':');
            try
            {
                return parts.Length switch
                {
                    2 => int.Parse(parts[0]) * 60 + int.Parse(parts[1]),
                    3 => int.Parse(parts[0]) * 3600 + int.Parse(parts[1]) * 60 + int.Parse(parts[2]),
                    _ => null
                };
            }
            catch
            {
                return null;
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
