using AIEduPlatform.Core.DTOs.ML_Health;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AIEduPlatform.ML.Services.health
{
    public class AIServiceHealthCheck : IHealthCheck
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<AIServiceHealthCheck> _logger;

        public AIServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            IOptions<AIServiceSettings> settings,
            ILogger<AIServiceHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            _logger.LogDebug("CheckHealthAsync: starting health check for all AI services.");

            var data = new Dictionary<string, object>();

            try
            {
                var embeddingHealth = await CheckServiceHealthAsync(
                    "EmbeddingService",
                    _settings.BaseUrls.EmbeddingService,
                    _settings.Embeddings.Health.Basic,
                    cancellationToken);

                data["embedding_service"] = embeddingHealth;

                var rerankingHealth = await CheckServiceHealthAsync(
                    "RerankingService",
                    _settings.BaseUrls.RerankingService,
                    _settings.Reranker.Health.Basic,
                    cancellationToken);

                data["reranking_service"] = rerankingHealth;

                var ollamaHealth = await CheckServiceHealthAsync(
                    "OllamaService",
                    _settings.BaseUrls.OllamaService,
                    _settings.Ollama.Health.Basic,
                    cancellationToken);

                data["ollama_service"] = ollamaHealth;

                var visionHealth = await CheckServiceHealthAsync(
                    "VisionService",
                    _settings.BaseUrls.VisionService,
                    _settings.Vision.Health.Basic,
                    cancellationToken);

                data["vision_service"] = visionHealth;

                var transcriptionHealth = await CheckServiceHealthAsync(
                    "TranscriptionService",
                    _settings.BaseUrls.TranscriptionService,
                    _settings.Transcription.Health.Basic,
                    cancellationToken);

                data["transcription_service"] = transcriptionHealth;

                var isHealthy = embeddingHealth.IsHealthy && rerankingHealth.IsHealthy && ollamaHealth.IsHealthy && visionHealth.IsHealthy && transcriptionHealth.IsHealthy;

                if (isHealthy)
                {
                    _logger.LogDebug("CheckHealthAsync: all AI services are healthy.");

                    return HealthCheckResult.Healthy(
                        "All AI services are healthy",
                        data);
                }
                else if (embeddingHealth.IsHealthy || rerankingHealth.IsHealthy || ollamaHealth.IsHealthy || visionHealth.IsHealthy || transcriptionHealth.IsHealthy)
                {
                    _logger.LogWarning("CheckHealthAsync: degraded. Embedding={Embedding}, Reranking={Reranking}, Ollama={Ollama}, Vision={Vision}, Transcription={Transcription}",
                        embeddingHealth.IsHealthy, rerankingHealth.IsHealthy, ollamaHealth.IsHealthy, visionHealth.IsHealthy, transcriptionHealth.IsHealthy);

                    return HealthCheckResult.Degraded(
                        "One or more AI services are unhealthy",
                        data: data);
                }
                else
                {
                    _logger.LogError("CheckHealthAsync: all AI services are unhealthy.");

                    return HealthCheckResult.Unhealthy(
                        "All AI services are unhealthy",
                        data: data);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CheckHealthAsync: health check failed with exception.");

                return HealthCheckResult.Unhealthy(
                    "Health check failed",
                    ex,
                    data);
            }
        }

        private async Task<ServiceHealthInfo> CheckServiceHealthAsync(
            string serviceName,
            string baseUrl,
            string healthEndpoint,
            CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _settings.Timeouts.HealthCheckTimeout;

            var url = $"{baseUrl}{healthEndpoint}";
            _logger.LogDebug("CheckServiceHealthAsync: checking {ServiceName} at {Url}.", serviceName, url);

            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.GetAsync(url, cancellationToken);

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    try
                    {
                        var healthData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

                        var status = healthData?.ContainsKey("status") == true
                            ? healthData["status"].GetString()
                            : "healthy";

                        _logger.LogDebug("CheckServiceHealthAsync: {ServiceName} is healthy. Status={Status}, ResponseTimeMs={ResponseTime}",
                            serviceName, status, stopwatch.ElapsedMilliseconds);

                        return new ServiceHealthInfo
                        {
                            IsHealthy = true,
                            Status = status,
                            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                            ServiceName = serviceName,
                            Url = url
                        };
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogDebug(ex, "CheckServiceHealthAsync: {ServiceName} returned success but body was not valid JSON, treating as healthy.",
                            serviceName);

                        return new ServiceHealthInfo
                        {
                            IsHealthy = true,
                            Status = "healthy",
                            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                            ServiceName = serviceName,
                            Url = url
                        };
                    }
                }
                else
                {
                    _logger.LogWarning("CheckServiceHealthAsync: {ServiceName} returned {StatusCode} at {Url}.",
                        serviceName, (int)response.StatusCode, url);

                    return new ServiceHealthInfo
                    {
                        IsHealthy = false,
                        Status = $"unhealthy (HTTP {(int)response.StatusCode})",
                        ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                        ServiceName = serviceName,
                        Url = url,
                        ErrorMessage = $"Status code: {response.StatusCode}"
                    };
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "CheckServiceHealthAsync: {ServiceName} is unreachable at {Url}.", serviceName, url);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "unreachable",
                    ServiceName = serviceName,
                    Url = url,
                    ErrorMessage = $"Connection failed: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogWarning(ex, "CheckServiceHealthAsync: {ServiceName} timed out at {Url}.", serviceName, url);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "timeout",
                    ServiceName = serviceName,
                    Url = url,
                    ErrorMessage = "Request timeout"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CheckServiceHealthAsync: unexpected error for {ServiceName} at {Url}.", serviceName, url);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "error",
                    ServiceName = serviceName,
                    Url = url,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}