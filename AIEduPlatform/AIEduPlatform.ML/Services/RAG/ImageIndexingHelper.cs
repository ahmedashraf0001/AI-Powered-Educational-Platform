using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Concept;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;

namespace AIEduPlatform.ML.Services.RAG
{
    public class ImageIndexingHelper : MaterialIndexingHelperBase
    {
        private readonly IVisionService _visionService;
        private readonly IFileService _fileService;
        private readonly SemaphoreSlim _visionSemaphore;

        public ImageIndexingHelper(
            IVisionService visionService,
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            IFileService fileService,
            IOptions<RagSettings> options,
            IConceptExtractionService conceptExtractionService,
            ILogger<ImageIndexingHelper> logger,
            IOllamaServiceClient summaryService)
            : base(embeddingService, serviceProvider, conceptExtractionService, options.Value, logger, summaryService)
        {
            _visionService = visionService;
            _fileService = fileService;
            _visionSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentVisionCalls,
                _ragSettings.Concurrency.MaxConcurrentVisionCalls);
        }

        public async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks, List<ChunkConceptsResult> conceptExtractions)> IndexImageAsync(
            Course course,
            Material material,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexImageAsync started: MaterialId={MaterialId}, Title={Title}",
                material.Id, material.Title);


            try
            {
                var metadata = CreateChunkMetadata(course, material);

                using var imageStream = await _fileService.DownloadFileAsync(material.FileUrl, cancellationToken);

                if (imageStream == null)
                {
                    _logger.LogWarning("IndexImageAsync: failed to download image. MaterialId={MaterialId}, Title={Title}",
                        material.Id, material.Title);
                    throw new InvalidOperationException("Failed to download image file");
                }
                ContextChunk chunkContext = null;
                ChunkingResult embedRequest = null;

                await _visionSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                try
                {
                    var imgInterpretation = await _visionService.ExtractInfoFromImageAsync(
                    imageStream,
                    cancellationToken);

                    if (string.IsNullOrWhiteSpace(imgInterpretation.DetailedCaption))
                    {
                        _logger.LogWarning("IndexImageAsync: vision service returned empty interpretation. MaterialId={MaterialId}",
                            material.Id);
                        throw new InvalidOperationException("Vision service returned empty interpretation");
                    }
                    chunkContext = new ContextChunk
                    {
                        Content = imgInterpretation.DetailedCaption,
                        Metadata = metadata,
                        RelevanceScore = 0f,
                        AdditionalData = new Dictionary<string, object>
                        {
                            ["prompt_used"] = imgInterpretation.PromptUsed,
                            ["image_dimensions"] = imgInterpretation.ImageDimensions,
                            ["wordCount"] = imgInterpretation.DetailedCaption.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                            ["model_name"] = imgInterpretation.ModelName,
                            ["light_description"] = imgInterpretation.Description
                        }
                    };
                    embedRequest = new ChunkingResult
                    {
                        Chunks = new List<ContextChunk> { chunkContext },
                        OriginalLength = imgInterpretation.DetailedCaption.Length,
                    };

                }
                finally
                {
                    _visionSemaphore.Release();
                }

                _logger.LogDebug("IndexImageAsync: sending chunk for embedding. MaterialId={MaterialId}",
                    metadata.MaterialId);

                var embedResult = await EmbedChunksAsync(embedRequest, metadata, options, cancellationToken);

                var allChunks = embedResult.materialChunks;
                var totalEmbeddingMs = embedResult.EmbeddingTimeMs;
                var failedChunks = embedResult.failedChunksCount;

                material.TotalPages = 1;

                var savedChunks = await SaveMaterialChunksAsync(allChunks, material, cancellationToken);
                var conceptExtractions = await ExtractConceptsFromChunksAsync(savedChunks, cancellationToken);

                _logger.LogInformation("IndexImageAsync completed: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}",
                    material.Id, material.Title, allChunks.Count, failedChunks, totalEmbeddingMs);

                return (savedChunks.Count, totalEmbeddingMs, failedChunks, conceptExtractions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexImageAsync failed: MaterialId={MaterialId}", material.Id);
                throw;
            }
        }
    }
}