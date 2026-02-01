using AIEduPlatform.Core.DTOs.ML_Health;
using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AIEduPlatform.ML.Services.health
{
    public class AIServiceHealthMonitor : IAIServiceHealthMonitor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<AIServiceHealthMonitor> _logger;

        public AIServiceHealthMonitor(
            IHttpClientFactory httpClientFactory,
            IOptions<AIServiceSettings> settings,
            ILogger<AIServiceHealthMonitor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<DetailedHealthResponse> GetEmbeddingServiceHealthAsync()
        {
            return await GetDetailedHealthAsync(
                _settings.BaseUrls.EmbeddingService,
                _settings.Embeddings.Health.Detailed);
        }

        public async Task<DetailedHealthResponse> GetRerankingServiceHealthAsync()
        {
            return await GetDetailedHealthAsync(
                _settings.BaseUrls.RerankingService,
                _settings.Reranker.Health.Detailed);
        }

        public async Task<DetailedHealthResponse> GetOllamaServiceHealthAsync()
        {
            return await GetDetailedHealthAsync(
                _settings.BaseUrls.OllamaService,
                _settings.Ollama.Health.Detailed);
        }

        public async Task<DetailedHealthResponse> GetVisionServiceHealthAsync()
        {
            return await GetDetailedHealthAsync(
                _settings.BaseUrls.VisionService,
                _settings.Vision.Health.Detailed);
        }

        public async Task<bool> IsEmbeddingServiceReadyAsync()
        {
            return await IsServiceReadyAsync(
                _settings.BaseUrls.EmbeddingService,
                _settings.Embeddings.Health.Ready);
        }

        public async Task<bool> IsRerankingServiceReadyAsync()
        {
            return await IsServiceReadyAsync(
                _settings.BaseUrls.RerankingService,
                _settings.Reranker.Health.Ready);
        }

        public async Task<bool> IsOllamaServiceReadyAsync()
        {
            return await IsServiceReadyAsync(
                _settings.BaseUrls.OllamaService,
                _settings.Ollama.Health.Ready);
        }

        public async Task<bool> IsVisionServiceReadyAsync()
        {
            return await IsServiceReadyAsync(
                _settings.BaseUrls.VisionService,
                _settings.Vision.Health.Ready);
        }

        public async Task<ServiceStatus> GetOverallStatusAsync()
        {
            _logger.LogDebug("GetOverallStatusAsync: polling all services.");

            var embeddingReady = await IsEmbeddingServiceReadyAsync();
            var rerankingReady = await IsRerankingServiceReadyAsync();
            var ollamaReady = await IsOllamaServiceReadyAsync();
            var visionReady = await IsVisionServiceReadyAsync();

            var status = new ServiceStatus
            {
                EmbeddingServiceReady = embeddingReady,
                RerankingServiceReady = rerankingReady,
                OllamaServiceReady = ollamaReady,
                VisionServiceReady = visionReady,
                IsFullyOperational = embeddingReady && rerankingReady,
                Timestamp = DateTime.UtcNow
            };

            if (status.IsFullyOperational)
                _logger.LogDebug("GetOverallStatusAsync: all core services operational. Ollama={Ollama}, Vision={Vision}",
                    ollamaReady, visionReady);
            else
                _logger.LogWarning("GetOverallStatusAsync: not fully operational. Embedding={Embedding}, Reranking={Reranking}, Ollama={Ollama}, Vision={Vision}",
                    embeddingReady, rerankingReady, ollamaReady, visionReady);

            return status;
        }

        private async Task<DetailedHealthResponse> GetDetailedHealthAsync(
            string baseUrl,
            string endpoint)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _settings.Timeouts.HealthCheckTimeout;

            var url = $"{baseUrl}{endpoint}";
            _logger.LogDebug("GetDetailedHealthAsync: requesting {Url}", url);

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var health = JsonSerializer.Deserialize<DetailedHealthResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                _logger.LogDebug("GetDetailedHealthAsync: {Url} responded successfully.", url);
                return health;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "GetDetailedHealthAsync: HTTP request failed for {Url}.", url);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "GetDetailedHealthAsync: failed to deserialize response from {Url}.", url);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetDetailedHealthAsync: unexpected error for {Url}.", url);
                throw;
            }
        }

        private async Task<bool> IsServiceReadyAsync(string baseUrl, string endpoint)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _settings.Timeouts.HealthCheckTimeout;

            var url = $"{baseUrl}{endpoint}";
            _logger.LogDebug("IsServiceReadyAsync: checking {Url}", url);

            try
            {
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogDebug("IsServiceReadyAsync: {Url} is ready.", url);
                    return true;
                }

                _logger.LogWarning("IsServiceReadyAsync: {Url} returned {StatusCode}.", url, (int)response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IsServiceReadyAsync: readiness check failed for {Url}.", url);
                return false;
            }
        }
    }
}