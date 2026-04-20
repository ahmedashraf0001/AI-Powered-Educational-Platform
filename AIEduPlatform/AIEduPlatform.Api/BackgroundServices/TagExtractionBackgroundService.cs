using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.BackgroundServices
{
    public class TagExtractionBackgroundService : BackgroundService
    {
        private readonly ITagExtractionQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TagExtractionBackgroundService> _logger;

        public TagExtractionBackgroundService(
            ITagExtractionQueue queue,
            IServiceProvider serviceProvider,
            ILogger<TagExtractionBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("TagExtractionBackgroundService started.");

            await foreach (var request in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation("Processing tag extraction request for CourseId={CourseId}, UserId={UserId}", request.CourseId, request.UserId);

                    using var scope = _serviceProvider.CreateScope();
                    var tagService = scope.ServiceProvider.GetRequiredService<ITagExtractionService>();
                    var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                    string courseTitle = "Your Course";

                    try
                    {
                        var result = await tagService.ExtractCourseTagsAsync(request.CourseId, stoppingToken);

                        _logger.LogInformation("Tag extraction completed for CourseId={CourseId}. Tags extracted: {TagCount}", request.CourseId, result.Tags?.Count ?? 0);

                        await notificationService.NotifyTagExtractionCompletedAsync(
                            request.UserId,
                            result.CourseId.ToString(), // Default back to course ID if title missing
                            true,
                            $"Extracted {result.Tags?.Count ?? 0} tags.",
                            stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error executing tag extraction for CourseId={CourseId}", request.CourseId);

                        await notificationService.NotifyTagExtractionCompletedAsync(
                            request.UserId,
                            courseTitle,
                            false,
                            ex.Message,
                            stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Tag extraction operation cancelled for CourseId={CourseId}", request.CourseId);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Catastrophic error handling tag extraction queue.");
                }
            }
        }
    }
}
