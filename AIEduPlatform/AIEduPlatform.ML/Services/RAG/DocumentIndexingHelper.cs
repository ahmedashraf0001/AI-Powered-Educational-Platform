using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Pdf;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.MaterialProcessing;
using AIEduPlatform.ML.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AIEduPlatform.ML.Services.RAG
{
    public class DocumentIndexingHelper : MaterialIndexingHelperBase
    {
        private readonly IDocumentContentExtractor _chunker;
        private readonly IVisionService _visionService;
        private readonly IFileService _fileService;
        private readonly SemaphoreSlim _visionSemaphore;
        public DocumentIndexingHelper(
            IDocumentContentExtractor chunker,
            IVisionService visionService,
            IFileService fileService,
            IEmbeddingService embeddingService,
            IServiceProvider serviceProvider,
            IOptions<RagSettings> options,
            ILogger<DocumentIndexingHelper> logger)
            : base(embeddingService, serviceProvider, options.Value, logger)
        {
            _chunker = chunker;
            _visionService = visionService;
            _fileService = fileService;

            _visionSemaphore = new SemaphoreSlim(
                _ragSettings.Concurrency.MaxConcurrentVisionCalls,
                _ragSettings.Concurrency.MaxConcurrentVisionCalls);
        }

        public ChunkingResult ChunkDocument(PageContent content, ChunkMetadata metadata, ChunkingOptions? options = null)
        {
            _logger.LogDebug("Chunking document: Source={Source}, Page={Page}, CustomOptions={HasOptions}",
                metadata.SourceTitle, content.PageNumber, options != null);

            if (options != null)
                _chunker.ResizeChunk(options);

            var chunks = _chunker.ChunkPageContent(content, metadata);
            var result = new ChunkingResult { Chunks = chunks, OriginalLength = content.WordCount};

            _logger.LogDebug("Chunking complete: Source={Source}, Page={Page}, ChunksProduced={Count}",
                metadata.SourceTitle, content.PageNumber, result.Chunks.Count);

            return result;
        }

        public async Task<(int numOfChunksIndexed, long totalEmbeddingMs, int failedChunks)> IndexDocumentAsync(
            Course course,
            Material material,
            ChunkingOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("IndexDocumentAsync started: MaterialId={MaterialId}, Title={Title}",
                material.Id, material.Title);

            try
            {
                if (options != null)
                {
                    _chunker.ResizeChunk(options);
                }

                var metadata = CreateChunkMetadata(course, material);
                var physicalPath = _fileService.ResolvePhysicalPath(material.FileUrl);

                using (var pdfReader = new PdfContentExtractor(physicalPath, _visionSemaphore, _visionService))
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

                    var pageTasks = pages.Select(async page =>
                    {
                        _logger.LogDebug("Processing page started: MaterialId={MaterialId}, Page={Page}",
                            metadata.MaterialId, page.PageNumber);

                        ChunkingResult pageChunks = ChunkDocument(page, metadata, options);

                        if (!pageChunks.Chunks.Any())
                        {
                            _logger.LogWarning("No chunks produced for page. MaterialId={MaterialId}, Page={Page}",
                                metadata.MaterialId, page.PageNumber);
                            throw new Exception("Failed to fetch page chunks");
                        }

                        _logger.LogDebug("Sending {ChunkCount} chunks for embedding. MaterialId={MaterialId}, Page={Page}",
                            pageChunks.Chunks.Count, metadata.MaterialId, page.PageNumber);

                        return await EmbedChunksAsync(pageChunks, metadata, options, cancellationToken);
                    });

                    var pageResults = await Task.WhenAll(pageTasks);

                    var allChunks = pageResults.SelectMany(r => r.materialChunks).ToList();
                    var totalEmbeddingMs = pageResults.Sum(r => r.EmbeddingTimeMs);
                    var failedChunks = pageResults.Sum(r => r.failedChunksCount);

                    await SaveMaterialChunksAsync(allChunks, material, cancellationToken);

                    _logger.LogInformation("IndexDocumentAsync completed: MaterialId={MaterialId}, Title={Title}, ChunksIndexed={Indexed}, ChunksFailed={Failed}, EmbeddingTimeMs={EmbedMs}",
                        material.Id, material.Title, allChunks.Count, failedChunks, totalEmbeddingMs);

                    return (allChunks.Count, totalEmbeddingMs, failedChunks);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IndexDocumentAsync failed: MaterialId={MaterialId}", material.Id);
                throw;
            }
        }     
    }
}
