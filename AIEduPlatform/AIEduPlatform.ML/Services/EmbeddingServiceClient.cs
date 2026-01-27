using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services
{
    public class EmbeddingServiceClient:IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        public EmbeddingServiceClient(HttpClient httpClient, IOptions<AIServiceSettings> settings) //ILogger<EmbeddingServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }
        public async Task<BatchEmbeddingResponse> GetBatchEmbeddingAsync(BatchEmbeddingRequest request, CancellationToken ct)
        {
            try
            {
                var url = _settings.Embeddings.Urls.Batch;

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<BatchEmbeddingResponse>(ct)
                    ?? throw new InvalidOperationException("Embedding API returned empty response");
                return result!;
            }
            catch (HttpRequestException ex)
            {
                //_logger.LogError(ex, "Failed to get batch embeddings from service");
                throw new ServiceUnavailableException("Embedding service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                //_logger.LogError(ex, "Batch embedding request timed out");
                throw new TimeoutException("Embedding service timed out", ex);
            }

        }

        public async Task<EmbeddingResponse> GetEmbeddingAsync(EmbeddingRequest request, CancellationToken ct)
        {
            try
            {
                var url = _settings.Embeddings.Urls.Single;

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(ct)
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
