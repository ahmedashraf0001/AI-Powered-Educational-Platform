using AIEduPlatform.Core.DTOs.ML_Health;
using AIEduPlatform.Core.Interfaces.Monitors;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace AIEduPlatform.ML.Services.health
{
    // Health check service
   
    public class AIServiceHealthMonitor : IAIServiceHealthMonitor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AIServiceSettings _settings;
        //private readonly ILogger<AIServiceHealthMonitor> _logger;

        public AIServiceHealthMonitor(
            IHttpClientFactory httpClientFactory,
            IOptions<AIServiceSettings> settings)
            //ILogger<AIServiceHealthMonitor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings.Value;
            //_logger = logger;
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

        public async Task<ServiceStatus> GetOverallStatusAsync()
        {
            var embeddingReady = await IsEmbeddingServiceReadyAsync();
            var rerankingReady = await IsRerankingServiceReadyAsync();

            return new ServiceStatus
            {
                EmbeddingServiceReady = embeddingReady,
                RerankingServiceReady = rerankingReady,
                IsFullyOperational = embeddingReady && rerankingReady,
                Timestamp = DateTime.UtcNow
            };
        }

        private async Task<DetailedHealthResponse> GetDetailedHealthAsync(
            string baseUrl,
            string endpoint)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _settings.Timeouts.HealthCheckTimeout;

            try
            {
                var url = $"{baseUrl}{endpoint}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DetailedHealthResponse>(
                    content,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Failed to get detailed health from {Url}", baseUrl);
                throw;
            }
        }

        private async Task<bool> IsServiceReadyAsync(string baseUrl, string endpoint)
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = _settings.Timeouts.HealthCheckTimeout;

            try
            {
                var url = $"{baseUrl}{endpoint}";
                var response = await client.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Readiness check failed for {Url}", baseUrl);
                return false;
            }
        }
    }
}
