using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services.Models
{
    public class VisionServiceClient : IVisionService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<VisionServiceClient> _logger;

        public VisionServiceClient(
            HttpClient httpClient,
            IOptions<AIServiceSettings> settings,
            ILogger<VisionServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<string> ExtractTextFromImageAsync(
            byte[] imageData,
            CancellationToken ct = default)
        {
            if (imageData == null || imageData.Length == 0)
                throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));

            try
            {
                var url = _settings.Vision.Urls.AnalyzeBytes;

                _logger.LogDebug("Requesting text extraction from image of size {Size} bytes", imageData.Length);

                var response = await _httpClient.PostAsJsonAsync(url, imageData, ct);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<string>(ct);

                if (result == null)
                {
                    _logger.LogError("Vision API returned null response");
                    throw new InvalidOperationException("Vision API returned empty response");
                }

                _logger.LogInformation(
                    "Successfully extracted text from image, result length: {Length} characters",
                    result.Length);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to extract text from image via vision service");
                throw new ServiceUnavailableException("Vision service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Vision text extraction request timed out");
                throw new TimeoutException("Vision service timed out", ex);
            }
        }
    }
}