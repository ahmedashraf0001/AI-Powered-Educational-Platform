using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services.Models
{
    public class RerankingServiceClient : IRerankingService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;

        public RerankingServiceClient(HttpClient HttpClient, IOptions<AIServiceSettings> settings) //ILogger<EmbeddingServiceClient> logger)
        {
            _httpClient = HttpClient;
            _settings = settings.Value;
        }
        public async Task<RerankResponse> RerankAsync(RerankRequest request, CancellationToken ct)
        {
            try
            {
                var url = _settings.Reranker.Urls.Rerank;

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RerankResponse>(ct)
                    ?? throw new InvalidOperationException("Embedding API returned empty response");
                return result;
            }
            catch (HttpRequestException ex)
            {
                //_logger.LogError(ex, "Failed to get embedding from service");
                throw new ServiceUnavailableException("Embedding service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                //_logger.LogError(ex, "Embedding request timed out");
                throw new TimeoutException("Embedding service timed out", ex);
            }

        }
        public async Task<RerankScorePairsResponse> RerankScorePairsAsync(RerankScorePairsRequest request, CancellationToken ct)
        {
            try
            {
                var url = _settings.Reranker.Urls.ScorePairs;

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RerankScorePairsResponse>(ct)
                    ?? throw new InvalidOperationException("Embedding API returned empty response");
                return result;
            }
            catch (HttpRequestException ex)
            {
                //_logger.LogError(ex, "Failed to get embedding from service");
                throw new ServiceUnavailableException("Embedding service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                //_logger.LogError(ex, "Embedding request timed out");
                throw new TimeoutException("Embedding service timed out", ex);
            }
        }
    }
}
