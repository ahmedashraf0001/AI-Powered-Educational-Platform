using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.BackgroundServices
{
    public class MaterialIndexingBackgroundService : BackgroundService
    {
        private readonly IMaterialIndexingQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MaterialIndexingBackgroundService> _logger;

        public MaterialIndexingBackgroundService(
            IMaterialIndexingQueue queue,
            IServiceProvider serviceProvider,
            ILogger<MaterialIndexingBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("MaterialIndexingBackgroundService started.");

            await foreach (var request in _queue.DequeueAllAsync(stoppingToken))
            {
                RagIndexResponse? response = null;

                try
                {
                    _logger.LogInformation(
                        "Processing indexing request for CourseId={CourseId}, UserId={UserId}",
                        request.CourseId, request.UserId);

                    using var scope = _serviceProvider.CreateScope();
                    var ragService = scope.ServiceProvider.GetRequiredService<IRAGService>();

                    // Execute indexing
                    response = await ragService.IndexAsync(new RagIndexRequest
                    {
                        CourseId = request.CourseId
                    }, stoppingToken);

                    _logger.LogInformation(
                        "Indexing completed for CourseId={CourseId}. Success: {Success}, ChunksIndexed: {ChunksIndexed}",
                        request.CourseId, response.Success, response.ChunksIndexed);

                    // Send success notification
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.NotifyIndexingCompletedAsync(
                        request.UserId,
                        response,
                        stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Indexing operation cancelled for CourseId={CourseId}", request.CourseId);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing indexing request for CourseId={CourseId}",
                        request.CourseId);

                    // Send failure notification
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var errorResponse = new RagIndexResponse
                        {
                            Success = false,
                            Error = ex.Message,
                            CourseId = request.CourseId,
                            ChunksIndexed = 0,
                            ChunksFailed = 0
                        };

                        await notificationService.NotifyIndexingCompletedAsync(
                            request.UserId,
                            errorResponse,
                            stoppingToken);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx,
                            "Failed to send error notification for CourseId={CourseId}",
                            request.CourseId);
                    }
                }
            }

            _logger.LogInformation("MaterialIndexingBackgroundService stopped.");
        }
    }
}