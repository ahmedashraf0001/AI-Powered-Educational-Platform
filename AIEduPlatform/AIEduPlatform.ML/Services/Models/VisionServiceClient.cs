using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace AIEduPlatform.ML.Services.Models
{
    public class VisionServiceClient : IVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        public VisionServiceClient(HttpClient httpClient, IOptions<AIServiceSettings> settings) //ILogger<EmbeddingServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }
        public async Task<string> ExtractTextFromImageAsync(byte[] imageData, CancellationToken ct)
        {
            try
            {
                var url = _settings.Vision.Urls.AnalyzeBytes;

                var response = await _httpClient.PostAsJsonAsync(url, imageData, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<string>(ct)
                    ?? throw new InvalidOperationException("Vision API returned empty response");
                return result!;
            }
            catch (HttpRequestException ex)
            {
                //_logger.LogError(ex, "Failed to get batch embeddings from service");
                throw new ServiceUnavailableException("Vision service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                //_logger.LogError(ex, "Batch embedding request timed out");
                throw new TimeoutException("Vision service timed out", ex);
            }

        }
    }
}
