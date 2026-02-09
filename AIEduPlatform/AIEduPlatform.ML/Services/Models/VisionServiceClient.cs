using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Vision;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

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

        public async Task<VisionAnalysisResponse> ExtractInfoFromImageAsync(
            Stream imageData,
            CancellationToken ct = default)
        {
            if (imageData == null)
                throw new ArgumentNullException(nameof(imageData));

            if (!imageData.CanRead)
                throw new ArgumentException("Image stream is not readable.", nameof(imageData));

            if (imageData.CanSeek && imageData.Length == 0)
                throw new ArgumentException("Image stream is empty.", nameof(imageData));
            try
            {
                var url = _settings.Vision.Urls.AnalyzeBase64;

                var size = imageData.CanSeek ? imageData.Length : -1;

                _logger.LogDebug(
                    "Requesting text extraction from image. Size={Size}",
                    size >= 0 ? size.ToString() : "Unknown");

                if (imageData.CanSeek)
                    imageData.Position = 0;

                using var memoryStream = new MemoryStream();
                await imageData.CopyToAsync(memoryStream, ct);

                var buffer = memoryStream.GetBuffer();
                var base64Image = Convert.ToBase64String(buffer, 0, (int)memoryStream.Length);

                var request = new VisionAnalysisRequest
                {
                    Image = base64Image,
                    Prompt = _settings.Vision.Configurations.StaticPrompt,
                    IncludeDetails = true,
                    IncludeMetadata = false
                };

                var response = await _httpClient.PostAsJsonAsync(url, request, ct);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(ct);
                    _logger.LogError("Vision API returned error: {StatusCode}, Content: {Content}",
                        response.StatusCode, errorContent);
                    response.EnsureSuccessStatusCode();
                }

                var result = await response.Content.ReadFromJsonAsync<VisionAnalysisResponse>(ct);

                if (result == null || string.IsNullOrWhiteSpace(result.DetailedCaption))
                {
                    _logger.LogError("Vision API returned null or empty detailed caption");
                    throw new InvalidOperationException("Vision API returned empty response");
                }

                _logger.LogInformation(
                    "Successfully extracted text from image ({Size} bytes). " +
                    "Model: {Model}, ProcessingTime: {ProcessingTime}ms, " +
                    "Dimensions: {Width}x{Height}, Result length: {Length} characters",
                    imageData.Length,
                    result.ModelName,
                    result.ProcessingTimeMs,
                    result.ImageDimensions?.Width,
                    result.ImageDimensions?.Height,
                    result.DetailedCaption.Length);

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