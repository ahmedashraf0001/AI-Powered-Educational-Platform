using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Reranking;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services.Models
{
    public class RerankingServiceClient : IRerankingService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<RerankingServiceClient> _logger;

        public RerankingServiceClient(
            HttpClient httpClient,
            IOptions<AIServiceSettings> settings,
            ILogger<RerankingServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<RerankResponse> RerankAsync(
            RerankRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Query))
                throw new ArgumentException("Query cannot be null or empty.", nameof(request.Query));

            if (request.Chunks == null || !request.Chunks.Any())
                throw new ArgumentException("Chunks cannot be null or empty.", nameof(request.Chunks));

            try
            {
                var url = _settings.Reranker.Urls.Rerank;

                _logger.LogDebug(
                    "Requesting reranking for query with {Count} chunks, TopK: {TopK}",
                    request.Chunks.Count,
                    request.TopK);

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RerankResponse>(ct);

                if (result == null)
                {
                    _logger.LogError("Reranking API returned null response");
                    throw new InvalidOperationException("Reranking API returned empty response");
                }

                _logger.LogInformation(
                    "Successfully reranked {Count} results using model {Model}",
                    result.Count,
                    result.Model);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get reranking from service");
                throw new ServiceUnavailableException("Reranking service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Reranking request timed out");
                throw new TimeoutException("Reranking service timed out", ex);
            }
        }

        public async Task<RerankScorePairsResponse> RerankScorePairsAsync(
            RerankScorePairsRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Pairs == null || !request.Pairs.Any())
                throw new ArgumentException("Pairs cannot be null or empty.", nameof(request.Pairs));

            try
            {
                var url = _settings.Reranker.Urls.ScorePairs;

                _logger.LogDebug("Requesting score pairs for {Count} pairs", request.Pairs.Count);

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<RerankScorePairsResponse>(ct);

                if (result == null)
                {
                    _logger.LogError("Score pairs API returned null response");
                    throw new InvalidOperationException("Score pairs API returned empty response");
                }

                _logger.LogInformation(
                    "Successfully scored {Count} pairs using model {Model}",
                    result.Results?.Count ?? 0,
                    result.Model);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get score pairs from service");
                throw new ServiceUnavailableException("Reranking service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Score pairs request timed out");
                throw new TimeoutException("Reranking service timed out", ex);
            }
        }
    }
}