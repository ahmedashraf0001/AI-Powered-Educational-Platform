using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.DTOs.Video;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xabe.FFmpeg;

namespace AIEduPlatform.ML.Services.RAG
{
    public class VideoIndexingHelper : MaterialIndexingHelperBase
    {
        private readonly IVideoService _videoAnalysisService;
        private readonly IFileService _fileService;
        private readonly SemaphoreSlim _videoAnalysisSemaphore;

        public VideoIndexingHelper(
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            IOptions<RagSettings> options,
            ILogger<VideoIndexingHelper> logger,
            IVideoService videoAnalysisService,
            IConceptExtractionService conceptExtractionService,
            IFileService fileService,
            IOllamaServiceClient summaryService) 
            : base(embeddingService, serviceProvider, conceptExtractionService, options.Value, logger, summaryService)
        {
            _videoAnalysisService = videoAnalysisService;
            _fileService = fileService;

            _videoAnalysisSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentVideoAnalysis,
                _ragSettings.Concurrency.MaxConcurrentVideoAnalysis);
        }
        public async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks, List<ChunkConceptsResult> conceptExtractions)> IndexVideoAsync(
           Course course,
           Material material,
           ChunkingOptions? options = null,
           CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexVideoAsync started: MaterialId={MaterialId}, Title={Title}",
                 material.Id, material.Title);

            FileStream? videoStreamData = null;

            try
            {
                var fileExtension = Path.GetExtension(material.FileUrl)?.TrimStart('.').ToLower();
                if (string.IsNullOrEmpty(fileExtension) ||
                    !Enum.TryParse<SupportedVideoFormats>(fileExtension, ignoreCase: true, out var _))
                {
                    throw new NotSupportedException($"Video format '{fileExtension}' is not supported.");
                }

                var metadata = CreateChunkMetadata(course, material);

                var fileSize = await _fileService.GetFileSizeAsync(material.FileUrl, cancellationToken);

                if (fileSize <= 0)
                {
                    _logger.LogWarning("Could not determine file size or file not found. MaterialId={MaterialId}", material.Id);
                    throw new InvalidOperationException("Could not determine video file size");
                }

                if (fileSize > _ragSettings.VideoProcessing.MaxFileSizeBytes)
                {
                    throw new InvalidOperationException(
                        $"Video file size ({fileSize} bytes) exceeds maximum allowed ({_ragSettings.VideoProcessing.MaxFileSizeBytes} bytes)");
                }

                var mediaInfo = await FFmpeg.GetMediaInfo(_fileService.ResolvePhysicalPath(material.FileUrl), cancellationToken);
                var duration = mediaInfo.Duration;

                if (duration.TotalSeconds <= 0)
                {
                    throw new InvalidOperationException("Video has invalid duration");
                }

                if (duration.TotalSeconds > _ragSettings.VideoProcessing.MaxDurationSeconds)
                {
                    throw new InvalidOperationException(
                        $"Video duration ({duration.TotalSeconds}s) exceeds maximum allowed ({_ragSettings.VideoProcessing.MaxDurationSeconds}s)");
                }

                var fps = mediaInfo.VideoStreams.FirstOrDefault()?.Framerate ?? 30;

                var (frameInterval, maxFrames) = CalculateFrameSampling(duration, fps, _ragSettings.VideoProcessing);

                _logger.LogDebug("IndexVideoAsync: calculated sampling - Duration={Duration}s, FPS={FPS}, FrameInterval={Interval}s, MaxFrames={MaxFrames}",
                     duration.TotalSeconds, fps, frameInterval, maxFrames);

                videoStreamData = await _fileService.DownloadFileAsync(material.FileUrl, cancellationToken);

                if (videoStreamData == null)
                {
                    _logger.LogWarning("IndexVideoAsync: failed to download video. MaterialId={MaterialId}, Title={Title}",
                        material.Id, material.Title);
                    throw new InvalidOperationException("Failed to download video file");
                }

                if (videoStreamData.CanSeek && videoStreamData.Position != 0)
                {
                    videoStreamData.Position = 0;
                }

                var analysisRequest = new VideoAnalysisRequest
                {
                    FrameIntervalSeconds = frameInterval,
                    MaxFrames = maxFrames,
                    Transcribe = true,
                    AnalyzeVisuals = true,
                    IncludeTimestamps = true,
                    SummaryFormat = false,
                    Language = "en"
                };

                var fileName = Path.GetFileName(material.FileUrl);
                VideoAnalysisResponse analysisResult = null;
                await _videoAnalysisSemaphore.WaitAsync(cancellationToken);
                try
                {
                   analysisResult = await _videoAnalysisService.AnalyzeVideoAsync(
                       videoStreamData,
                       analysisRequest,
                       fileName,
                       cancellationToken);

                    if (analysisResult?.Segments == null || analysisResult.Segments.Count == 0)
                    {
                        _logger.LogWarning("Video analysis returned no segments. MaterialId={MaterialId}", material.Id);
                        throw new InvalidOperationException("Video analysis returned no segments");
                    }
                }
                finally
                {
                    _videoAnalysisSemaphore.Release();
                }          

                var chunksContext = CreateChunksFromVideoAnalysis(analysisResult, metadata);

                var embedRequest = new ChunkingResult
                {
                    Chunks = chunksContext,
                    OriginalLength = chunksContext.Sum(e => e.Content.Length),
                };

                _logger.LogDebug("IndexVideoAsync: created {ChunkCount} chunks from video analysis. MaterialId={MaterialId}, Title={Title}",
                    chunksContext.Count, material.Id, material.Title);

                var embedResult = await EmbedChunksAsync(embedRequest, metadata, options, cancellationToken);

                var allChunks = embedResult.materialChunks;
                var totalEmbeddingMs = embedResult.EmbeddingTimeMs;
                var failedChunks = embedResult.failedChunksCount;

                var savedChunks = await SaveMaterialChunksAsync(allChunks, material, cancellationToken);
                var conceptExtractions = await ExtractConceptsFromChunksAsync(savedChunks, cancellationToken);


                _logger.LogInformation("IndexVideoAsync completed: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}",
                    material.Id, material.Title, allChunks.Count, failedChunks, totalEmbeddingMs);

                return (allChunks.Count, totalEmbeddingMs, failedChunks, conceptExtractions);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogWarning(ex, "Unsupported video format: MaterialId={MaterialId}", material.Id);
                throw;
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid video file: MaterialId={MaterialId}", material.Id);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in IndexVideoAsync: MaterialId={MaterialId}, Title={Title}",
                    material.Id, material.Title);
                throw;
            }
            finally
            {
                videoStreamData?.Dispose();
            }
        }
        private List<ContextChunk> CreateChunksFromVideoAnalysis(
            VideoAnalysisResponse analysisResult,
            ChunkMetadata metadata)
        {
            if (analysisResult?.Segments == null || analysisResult.Segments.Count == 0)
            {
                _logger.LogWarning("No segments in video analysis result");
                return new List<ContextChunk>();
            }

            var chunks = new List<ContextChunk>();

            for (int i = 0; i < analysisResult.Segments.Count; i++)
            {
                var segment = analysisResult.Segments[i];

                if (string.IsNullOrWhiteSpace(segment.VisualDescription) &&
                    string.IsNullOrWhiteSpace(segment.Transcript))
                {
                    _logger.LogDebug("Skipping empty segment at index {Index}", i);
                    continue;
                }

                var contentParts = new List<string>
                {
                    $"[{FormatTimestamp(segment.StartTime)} - {FormatTimestamp(segment.EndTime)}]"
                };

                if (!string.IsNullOrWhiteSpace(segment.VisualDescription))
                {
                    contentParts.Add($"Visual: {segment.VisualDescription.Trim()}");
                }

                if (!string.IsNullOrWhiteSpace(segment.Transcript))
                {
                    contentParts.Add($"Transcript: {segment.Transcript.Trim()}");
                }

                var content = string.Join("\n", contentParts);

                var chunk = new ContextChunk
                {
                    Content = content,
                    Metadata = metadata,
                    RelevanceScore = 0f,
                    AdditionalData = new Dictionary<string, object>
                    {
                        ["chunkType"] = "video_segment",
                        ["chunkIndex"] = i,
                        ["segmentNumber"] = chunks.Count + 1,
                        ["start_time"] = segment.StartTime,
                        ["end_time"] = segment.EndTime,
                        ["duration"] = segment.EndTime - segment.StartTime,
                        ["visual_description"] = segment.VisualDescription?.Trim() ?? string.Empty,
                        ["transcript"] = segment.Transcript?.Trim() ?? string.Empty,
                        ["wordCount"] = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                        ["hasVisual"] = !string.IsNullOrWhiteSpace(segment.VisualDescription),
                        ["hasTranscript"] = !string.IsNullOrWhiteSpace(segment.Transcript)
                    }
                };
                chunks.Add(chunk);
            }

            _logger.LogDebug("Created {ChunkCount} video chunks from {SegmentCount} segments",
                chunks.Count, analysisResult.Segments.Count);

            return chunks;
        }

        private string FormatTimestamp(float seconds)
        {
            var timeSpan = TimeSpan.FromSeconds(seconds);
            return timeSpan.ToString(@"hh\:mm\:ss");
        }
        private (float frameInterval, int maxFrames) CalculateFrameSampling(
            TimeSpan duration,
            double fps,
            VideoProcessingSettings? settings = null)
        {
            settings ??= new VideoProcessingSettings();

            var durationSeconds = (float)duration.TotalSeconds;

            if (durationSeconds <= 0)
            {
                return (5.0f, settings.TargetFrames);
            }

            float idealInterval = durationSeconds / settings.TargetFrames;
            float frameInterval = Math.Clamp(idealInterval, settings.MinIntervalSeconds, settings.MaxIntervalSeconds);

            int calculatedMaxFrames = (int)Math.Ceiling(durationSeconds / frameInterval);
            int maxFrames = Math.Clamp(calculatedMaxFrames, settings.MinFrames, settings.MaxFrames);

            if (calculatedMaxFrames > settings.MaxFrames)
            {
                frameInterval = durationSeconds / settings.MaxFrames;
            }

            return (frameInterval, maxFrames);
        }

    }
}
