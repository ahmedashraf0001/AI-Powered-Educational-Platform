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
                try
                {
                    _logger.LogInformation("Processing indexing request for CourseId={CourseId}", request.CourseId);

                    using var scope = _serviceProvider.CreateScope();
                    var ragService = scope.ServiceProvider.GetRequiredService<IRAGService>();

                    await ragService.IndexAsync(new RagIndexRequest
                    {
                        CourseId = request.CourseId
                    }, stoppingToken);

                    _logger.LogInformation("Indexing completed for CourseId={CourseId}", request.CourseId);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing indexing request for CourseId={CourseId}", request.CourseId);
                }
            }

            _logger.LogInformation("MaterialIndexingBackgroundService stopped.");
        }
    }
}
