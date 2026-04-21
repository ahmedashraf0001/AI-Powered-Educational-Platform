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
        private static readonly Regex PageNumberRegex = new(@"\d+", RegexOptions.Compiled);
        private static readonly Regex TimestampRangeInContentRegex = new(
            @"\[(?<start>\d{1,2}:\d{2}(?::\d{2})?)\s*-\s*(?<end>\d{1,2}:\d{2}(?::\d{2})?)\]",
            RegexOptions.Compiled);
        private static readonly Regex TimestampTokenRegex = new(@"\d{1,2}:\d{2}(?::\d{2})?", RegexOptions.Compiled);

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
                
                // Get a fresh reference from the db without deep navigation properties
                // to prevent EF cascading UpdateAsync to existing Lectures and Courses.
                var dbMaterial = await scopedUow.Materials.GetMaterialByIdAsync(material.Id, false, cancellationToken);
                if (dbMaterial != null)
                {
                    dbMaterial.Summary = summary.Content;
                    dbMaterial.Indexed = true;
                    dbMaterial.UpdatedAt = DateTime.UtcNow;
                    // Update only on the isolated dbMaterial instance (avoiding entity duplicate tracking)
                    await scopedUow.Materials.UpdateAsync(dbMaterial, cancellationToken);
                }

                // Update the separated material ref so callers see the change
                material.Summary = summary.Content;
                material.Indexed = true;
                material.UpdatedAt = DateTime.UtcNow;

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

                // Order chunks by numeric location (page/timestamp) instead of lexicographic location strings.
                var orderedChunks = savedChunks
                    .Select((chunk, index) => new { chunk, index })
                    .OrderBy(x => GetChunkSortKey(x.chunk, isTimeBased, x.index))
                    .Select(x => x.chunk)
                    .ToList();

                // Build a location-aware condensed representation so all material regions are visible to the LLM.
                var contentBuilder = new StringBuilder();
                AppendCoverageHint(contentBuilder, orderedChunks, material, isTimeBased);

                const int maxChunkExcerptLength = 280;
                foreach (var chunk in orderedChunks)
                {
                    var chunkContent = chunk.Content?.Replace("\0", string.Empty).Trim();
                    if (string.IsNullOrWhiteSpace(chunkContent))
                        continue;

                    var location = ResolveChunkLocation(chunk, isTimeBased);
                    if (!string.IsNullOrWhiteSpace(location))
                    {
                        contentBuilder.AppendLine($"Location: {location}");
                    }

                    if (chunkContent.Length > maxChunkExcerptLength)
                    {
                        chunkContent = chunkContent[..maxChunkExcerptLength] + " ...";
                    }

                    contentBuilder.AppendLine(chunkContent);
                    contentBuilder.AppendLine();
                }

                var content = contentBuilder.ToString();
                if (string.IsNullOrWhiteSpace(content))
                {
                    _logger.LogWarning("ExtractAndSaveSemanticSectionsAsync: no content to extract sections from. MaterialId={MaterialId}", material.Id);
                    return;
                }

                var result = await _summaryService.ExtractSemanticSectionsAsync(content, isTimeBased, cancellationToken);

                if (result?.Sections == null || !result.Sections.Any())
                {
                    _logger.LogWarning("ExtractAndSaveSemanticSectionsAsync: LLM returned no sections. MaterialId={MaterialId}", material.Id);
                    return;
                }

                var semanticSections = new List<SemanticSection>();
                int? previousTimeBoundary = null;
                int? previousPageBoundary = null;

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
                        var normalizedStart = NormalizeTimestampToken(s.Start) ?? s.Start;
                        var normalizedEnd = NormalizeTimestampToken(s.End) ?? s.End;

                        var startSeconds = ParseTimestampToSeconds(normalizedStart);
                        var endSeconds = ParseTimestampToSeconds(normalizedEnd);

                        if (!startSeconds.HasValue && endSeconds.HasValue)
                            startSeconds = endSeconds;

                        if (!endSeconds.HasValue && startSeconds.HasValue)
                            endSeconds = startSeconds;

                        if (previousTimeBoundary.HasValue && startSeconds.HasValue && startSeconds.Value < previousTimeBoundary.Value)
                        {
                            _logger.LogDebug(
                                "Normalized section start time to preserve monotonic order. MaterialId={MaterialId}, SectionIndex={SectionIndex}, OldStart={OldStart}, NewStart={NewStart}",
                                material.Id, i, startSeconds.Value, previousTimeBoundary.Value);
                            startSeconds = previousTimeBoundary.Value;
                        }

                        if (startSeconds.HasValue && endSeconds.HasValue && endSeconds.Value < startSeconds.Value)
                        {
                            _logger.LogDebug(
                                "Normalized section end time to be >= start. MaterialId={MaterialId}, SectionIndex={SectionIndex}, Start={Start}, OldEnd={OldEnd}",
                                material.Id, i, startSeconds.Value, endSeconds.Value);
                            endSeconds = startSeconds;
                        }

                        section.StartSeconds = startSeconds;
                        section.EndSeconds = endSeconds;

                        if (section.EndSeconds.HasValue)
                            previousTimeBoundary = section.EndSeconds.Value;
                        else if (section.StartSeconds.HasValue)
                            previousTimeBoundary = section.StartSeconds.Value;
                    }
                    else
                    {
                        var startPage = s.StartPage;
                        var endPage = s.EndPage;

                        if (!startPage.HasValue && endPage.HasValue)
                            startPage = endPage;

                        if (!endPage.HasValue && startPage.HasValue)
                            endPage = startPage;

                        if (startPage.HasValue && startPage.Value < 1)
                            startPage = 1;

                        if (endPage.HasValue && endPage.Value < 1)
                            endPage = 1;

                        if (previousPageBoundary.HasValue && startPage.HasValue && startPage.Value < previousPageBoundary.Value)
                        {
                            _logger.LogDebug(
                                "Normalized section start page to preserve monotonic order. MaterialId={MaterialId}, SectionIndex={SectionIndex}, OldStartPage={OldStartPage}, NewStartPage={NewStartPage}",
                                material.Id, i, startPage.Value, previousPageBoundary.Value);
                            startPage = previousPageBoundary.Value;
                        }

                        if (startPage.HasValue && endPage.HasValue && endPage.Value < startPage.Value)
                        {
                            _logger.LogDebug(
                                "Normalized section end page to be >= start page. MaterialId={MaterialId}, SectionIndex={SectionIndex}, StartPage={StartPage}, OldEndPage={OldEndPage}",
                                material.Id, i, startPage.Value, endPage.Value);
                            endPage = startPage;
                        }

                        section.StartPage = startPage;
                        section.EndPage = endPage;

                        if (section.EndPage.HasValue)
                            previousPageBoundary = section.EndPage.Value;
                        else if (section.StartPage.HasValue)
                            previousPageBoundary = section.StartPage.Value;
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

        private static string? NormalizeTimestampToken(string? rawTimestamp)
        {
            if (string.IsNullOrWhiteSpace(rawTimestamp))
                return null;

            var match = TimestampTokenRegex.Match(rawTimestamp);
            if (!match.Success)
                return null;

            var token = match.Value;
            var parts = token.Split(':', StringSplitOptions.TrimEntries);

            if (parts.Length == 2
                && int.TryParse(parts[0], out var minutes)
                && int.TryParse(parts[1], out var seconds))
            {
                return FormatTimestamp((minutes * 60) + seconds);
            }

            if (parts.Length == 3
                && int.TryParse(parts[0], out var hours)
                && int.TryParse(parts[1], out var mins)
                && int.TryParse(parts[2], out var secs))
            {
                return FormatTimestamp((hours * 3600) + (mins * 60) + secs);
            }

            return null;
        }

        private static double GetChunkSortKey(MaterialChunk chunk, bool isTimeBased, int fallbackIndex)
        {
            var location = ResolveChunkLocation(chunk, isTimeBased);

            if (isTimeBased)
            {
                if (TryParseTimeRange(location, out var startSeconds, out _))
                    return startSeconds + (fallbackIndex * 0.000001);
            }
            else
            {
                if (TryParsePageNumber(location, out var page))
                    return page + (fallbackIndex * 0.000001);
            }

            return 1_000_000 + fallbackIndex;
        }

        private static string? ResolveChunkLocation(MaterialChunk chunk, bool isTimeBased)
        {
            if (!string.IsNullOrWhiteSpace(chunk.PageOrTimestamp))
                return chunk.PageOrTimestamp.Trim();

            if (!isTimeBased || string.IsNullOrWhiteSpace(chunk.Content))
                return null;

            var match = TimestampRangeInContentRegex.Match(chunk.Content);
            if (!match.Success)
                return null;

            return $"{match.Groups["start"].Value} - {match.Groups["end"].Value}";
        }

        private static bool TryParsePageNumber(string? location, out int page)
        {
            page = 0;

            if (string.IsNullOrWhiteSpace(location))
                return false;

            var match = PageNumberRegex.Match(location);
            return match.Success && int.TryParse(match.Value, out page);
        }

        private static bool TryParseTimeRange(string? location, out int startSeconds, out int endSeconds)
        {
            startSeconds = 0;
            endSeconds = 0;

            if (string.IsNullOrWhiteSpace(location))
                return false;

            var normalized = location.Trim().Trim('[', ']');
            var parts = normalized.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return false;

            var parsedStart = ParseTimestampToSeconds(parts[0]);
            if (!parsedStart.HasValue)
                return false;

            var parsedEnd = parts.Length > 1 ? ParseTimestampToSeconds(parts[1]) : parsedStart;
            if (!parsedEnd.HasValue)
                parsedEnd = parsedStart;

            startSeconds = parsedStart.Value;
            endSeconds = parsedEnd.Value;
            return true;
        }

        private static void AppendCoverageHint(
            StringBuilder contentBuilder,
            List<MaterialChunk> orderedChunks,
            Material material,
            bool isTimeBased)
        {
            if (orderedChunks.Count == 0)
                return;

            if (isTimeBased)
            {
                int? start = null;
                int? end = null;

                foreach (var chunk in orderedChunks)
                {
                    var location = ResolveChunkLocation(chunk, true);
                    if (!TryParseTimeRange(location, out var chunkStart, out var chunkEnd))
                        continue;

                    start = !start.HasValue ? chunkStart : Math.Min(start.Value, chunkStart);
                    end = !end.HasValue ? chunkEnd : Math.Max(end.Value, chunkEnd);
                }

                if (material.DurationSeconds.HasValue)
                {
                    end = !end.HasValue
                        ? material.DurationSeconds.Value
                        : Math.Max(end.Value, material.DurationSeconds.Value);
                }

                if (start.HasValue && end.HasValue)
                {
                    contentBuilder.AppendLine($"CoverageRange: {FormatTimestamp(start.Value)} - {FormatTimestamp(end.Value)}");
                    contentBuilder.AppendLine();
                }

                return;
            }

            int? minPage = null;
            int? maxPage = null;
            foreach (var chunk in orderedChunks)
            {
                var location = ResolveChunkLocation(chunk, false);
                if (!TryParsePageNumber(location, out var page))
                    continue;

                minPage = !minPage.HasValue ? page : Math.Min(minPage.Value, page);
                maxPage = !maxPage.HasValue ? page : Math.Max(maxPage.Value, page);
            }

            if (material.TotalPages.HasValue)
            {
                maxPage = !maxPage.HasValue
                    ? material.TotalPages.Value
                    : Math.Max(maxPage.Value, material.TotalPages.Value);
            }

            if (minPage.HasValue && maxPage.HasValue)
            {
                contentBuilder.AppendLine($"CoverageRange: Page {minPage.Value} - Page {maxPage.Value}");
                contentBuilder.AppendLine();
            }
        }

        private static string FormatTimestamp(int totalSeconds)
            => TimeSpan.FromSeconds(Math.Max(0, totalSeconds)).ToString(@"hh\:mm\:ss");

        private static string TrimContentKeepingHeadAndTail(string content, int maxLength)
        {
            if (content.Length <= maxLength)
                return content;

            const string marker = "\n...\n[Middle content omitted for length]\n...\n";
            var keep = Math.Max(200, (maxLength - marker.Length) / 2);
            var head = content[..keep];
            var tail = content[^keep..];
            return head + marker + tail;
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
