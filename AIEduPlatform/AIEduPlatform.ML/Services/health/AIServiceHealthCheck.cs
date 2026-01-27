using AIEduPlatform.Core.DTOs.ML_Health;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Diagnostics.HealthChecks;
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
        //private readonly ILogger<AIServiceHealthCheck> _logger;

        public AIServiceHealthCheck(
            IHttpClientFactory httpClientFactory,
            IOptions<AIServiceSettings> settings)
            //ILogger<AIServiceHealthCheck> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            //_logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var data = new Dictionary<string, object>();

            try
            {
                // Check Embedding Service
                var embeddingHealth = await CheckServiceHealthAsync(
                    "EmbeddingService",
                    _settings.BaseUrls.EmbeddingService,
                    _settings.Embeddings.Health.Basic,
                    cancellationToken);

                data["embedding_service"] = embeddingHealth;

                // Check Reranking Service
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

                // Determine overall health
                var isHealthy = embeddingHealth.IsHealthy && rerankingHealth.IsHealthy && ollamaHealth.IsHealthy;

                if (isHealthy)
                {
                    return HealthCheckResult.Healthy(
                        "All AI services are healthy",
                        data);
                }
                else if (embeddingHealth.IsHealthy || rerankingHealth.IsHealthy || ollamaHealth.IsHealthy)
                {
                    return HealthCheckResult.Degraded(
                        "One or more AI services are unhealthy",
                        data: data);
                }
                else
                {
                    return HealthCheckResult.Unhealthy(
                        "All AI services are unhealthy",
                        data: data);
                }
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Health check failed with exception");
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

            try
            {
                var url = $"{baseUrl}{healthEndpoint}";
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var response = await client.GetAsync(url, cancellationToken);

                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);

                    try
                    {
                        var healthData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);

                        return new ServiceHealthInfo
                        {
                            IsHealthy = true,
                            Status = healthData?.ContainsKey("status") == true
                                ? healthData["status"].GetString()
                                : "healthy",
                            ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                            ServiceName = serviceName,
                            Url = url
                        };
                    }
                    catch (JsonException)
                    {
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
                    //_logger.LogWarning(
                    //    "Health check failed for {ServiceName} at {Url} with status code {StatusCode}",
                    //    serviceName, url, response.StatusCode);

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
                //_logger.LogError(ex,
                //    "HTTP request failed for {ServiceName} health check",
                //    serviceName);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "unreachable",
                    ServiceName = serviceName,
                    Url = $"{baseUrl}{healthEndpoint}",
                    ErrorMessage = $"Connection failed: {ex.Message}"
                };
            }
            catch (TaskCanceledException ex)
            {
                //_logger.LogError(ex,
                //    "Health check timeout for {ServiceName}",
                //    serviceName);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "timeout",
                    ServiceName = serviceName,
                    Url = $"{baseUrl}{healthEndpoint}",
                    ErrorMessage = "Request timeout"
                };
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex,
                //    "Unexpected error checking health for {ServiceName}",
                //    serviceName);

                return new ServiceHealthInfo
                {
                    IsHealthy = false,
                    Status = "error",
                    ServiceName = serviceName,
                    Url = $"{baseUrl}{healthEndpoint}",
                    ErrorMessage = ex.Message
                };
            }
        }
    }


}

