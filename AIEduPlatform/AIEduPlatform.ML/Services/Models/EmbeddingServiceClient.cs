using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services.Models
{
    public class EmbeddingServiceClient : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<EmbeddingServiceClient> _logger;

        public EmbeddingServiceClient(
            HttpClient httpClient,
            IOptions<AIServiceSettings> settings,
            ILogger<EmbeddingServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<BatchEmbeddingResponse> GetBatchEmbeddingAsync(
            BatchEmbeddingRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Texts == null || !request.Texts.Any())
                throw new ArgumentException("Texts cannot be null or empty.", nameof(request.Texts));

            try
            {
                var url = _settings.Embeddings.Urls.Batch;

                _logger.LogDebug("Requesting batch embeddings for {Count} texts", request.Texts.Count);

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BatchEmbeddingResponse>(ct);

                if (result == null)
                {
                    _logger.LogError("Embedding API returned null response");
                    throw new InvalidOperationException("Embedding API returned empty response");
                }

                _logger.LogInformation(
                    "Successfully generated {Successful} batch embeddings, {Failed} failed",
                    result.Successful,
                    result.Failed);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get batch embeddings from service");
                throw new ServiceUnavailableException("Embedding service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Batch embedding request timed out");
                throw new TimeoutException("Embedding service timed out", ex);
            }
        }

        public async Task<EmbeddingResponse> GetEmbeddingAsync(
            EmbeddingRequest request,
            CancellationToken ct = default)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Text))
                throw new ArgumentException("Text cannot be null or empty.", nameof(request.Text));

            try
            {
                var url = _settings.Embeddings.Urls.Single;

                _logger.LogDebug("Requesting single embedding for text of length {Length}", request.Text.Length);

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct);

                if (result == null)
                {
                    _logger.LogError("Embedding API returned null response");
                    throw new InvalidOperationException("Embedding API returned empty response");
                }

                _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions",
                    result.Embedding?.Count ?? 0);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get embedding from service");
                throw new ServiceUnavailableException("Embedding service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Embedding request timed out");
                throw new TimeoutException("Embedding service timed out", ex);
            }
        }
    }
}