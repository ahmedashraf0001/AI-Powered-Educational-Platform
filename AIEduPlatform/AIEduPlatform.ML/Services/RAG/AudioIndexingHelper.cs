using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.MaterialProcessing;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEduPlatform.ML.Services.RAG
{
    public class AudioIndexingHelper : MaterialIndexingHelperBase
    {
        private readonly IAudioTranscriptionChunker _audioChunker;
        private readonly ITranscriptionService _transcriptionService;
        private readonly IFileService _fileService;
        private readonly SemaphoreSlim _transcriptionSemaphore;

        public AudioIndexingHelper(
            IAudioTranscriptionChunker audioChunker,
            ITranscriptionService transcriptionService,
            IFileService fileService,
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            IOptions<RagSettings> options,
            ILogger<AudioIndexingHelper> logger)
            : base(embeddingService, serviceProvider, options.Value, logger)
        {
            _audioChunker = audioChunker;
            _transcriptionService = transcriptionService;
            _fileService = fileService;

            _transcriptionSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentTranscriptions,
                _ragSettings.Concurrency.MaxConcurrentTranscriptions);
        }

        public async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexAudioAsync(
            Course course,
            Material material,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexAudioAsync started: MaterialId={MaterialId}, Title={Title}",
                material.Id, material.Title);

            try
            {
                var metadata = CreateChunkMetadata(course, material);

                if (options != null)
                {
                    _audioChunker.ResizeChunk(options);
                }

                List<AudioChunk> audioChunks;
                var physicalPath = _fileService.ResolvePhysicalPath(material.FileUrl);
                using (var audioExtractor = new AudioContentExtractor(physicalPath))
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

                var allChunks = new List<MaterialChunk>();
                long totalEmbeddingMs = 0;
                int failedChunks = 0;

                var batchSize = _ragSettings.AudioProcessing.TranscriptionBatchSize;
                int totalBatches = (audioChunks.Count + batchSize - 1) / batchSize;

                for (int i = 0; i < audioChunks.Count; i += batchSize)
                {
                    var batch = audioChunks.Skip(i).Take(batchSize).ToList();
                    int currentBatch = (i / batchSize) + 1;

                    _logger.LogDebug("IndexAudioAsync: processing batch {BatchNumber}/{TotalBatches} with {ChunkCount} chunks",
                        currentBatch, totalBatches, batch.Count);

                    var (batchChunks, batchEmbeddingMs, batchFailedChunks) = await ProcessAudioBatchAsync(
                        batch,
                        metadata,
                        cancellationToken);

                    allChunks.AddRange(batchChunks);
                    totalEmbeddingMs += batchEmbeddingMs;
                    failedChunks += batchFailedChunks;

                    _logger.LogInformation("IndexAudioAsync: completed batch {BatchNumber}/{TotalBatches}. Total chunks so far: {ChunkCount}",
                        currentBatch, totalBatches, allChunks.Count);
                }

                await SaveMaterialChunksAsync(allChunks, material, cancellationToken);

                _logger.LogInformation("IndexAudioAsync completed: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}, TotalAudioDuration={Duration}s",
                    material.Id, material.Title, allChunks.Count, failedChunks, totalEmbeddingMs, audioChunks.Sum(c => c.DurationSeconds));

                return (allChunks.Count, totalEmbeddingMs, failedChunks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexAudioAsync failed: MaterialId={MaterialId}", material.Id);
                throw;
            }
        }

        private async Task<(List<MaterialChunk> chunks, long embeddingTimeMs, int failedChunks)> ProcessAudioBatchAsync(
            List<AudioChunk> audioBatch,
            ChunkMetadata metadata,
            CancellationToken cancellationToken)
        {
            var batchChunks = new List<MaterialChunk>();
            long batchEmbeddingMs = 0;
            int batchFailedChunks = 0;

            var transcriptionTasks = audioBatch.Select(async chunk =>
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
                _logger.LogError(ex, "ProcessAudioBatchAsync: batch transcription failed");
                return (batchChunks, batchEmbeddingMs, audioBatch.Count);
            }

            foreach (var transcriptionResult in transcriptionResults)
            {
                var audioChunk = audioBatch.FirstOrDefault(ac => ac.Index == transcriptionResult.AudioChunkIndex);

                if (audioChunk == null)
                {
                    _logger.LogWarning("ProcessAudioBatchAsync: no matching audio chunk found for transcription index {Index}",
                        transcriptionResult.AudioChunkIndex);
                    batchFailedChunks++;
                    continue;
                }

                try
                {
                    var textChunks = _audioChunker.ChunkTranscribedAudio(
                        transcribedText: transcriptionResult.TransResult.Text,
                        segments: transcriptionResult.TransResult.Segments,
                        baseMetadata: metadata,
                        audioChunkIndex: audioChunk.Index,
                        audioStartTime: audioChunk.StartTimeSeconds,
                        audioEndTime: audioChunk.StartTimeSeconds + audioChunk.DurationSeconds
                    );

                    _logger.LogDebug("ProcessAudioBatchAsync: created {ChunkCount} text chunks from audio chunk {Index}",
                        textChunks.Count, audioChunk.Index);

                    var chunkingResult = new ChunkingResult
                    {
                        Chunks = textChunks,
                        OriginalLength = transcriptionResult.TransResult.Text?.Length ?? 0
                    };

                    var embedResult = await EmbedChunksAsync(chunkingResult, metadata, null, cancellationToken);

                    batchChunks.AddRange(embedResult.materialChunks);
                    batchEmbeddingMs += embedResult.EmbeddingTimeMs;
                    batchFailedChunks += embedResult.failedChunksCount;

                    _logger.LogDebug("ProcessAudioBatchAsync: embedded {SuccessCount} chunks, {FailedCount} failed for audio chunk {Index}",
                        embedResult.materialChunks.Count, embedResult.failedChunksCount, audioChunk.Index);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ProcessAudioBatchAsync: failed to process transcription for audio chunk {Index}", audioChunk.Index);
                    batchFailedChunks++;
                }
            }

            return (batchChunks, batchEmbeddingMs, batchFailedChunks);
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
    }
}
