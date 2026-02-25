using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.Video;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace AIEduPlatform.ML.Services.Models
{
    public class VideoServiceClient : IVideoService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<VideoServiceClient> _logger;

        public VideoServiceClient(
            HttpClient httpClient,
            IOptions<AIServiceSettings> settings,
            ILogger<VideoServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task<VideoAnalysisResponse> AnalyzeVideoAsync(
            Stream videoStream,
            VideoAnalysisRequest request,
            string? fileName = null,
            CancellationToken ct = default)
        {
            if (videoStream == null)
                throw new ArgumentNullException(nameof(videoStream));

            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (!videoStream.CanRead)
                throw new ArgumentException("Video stream must be readable.", nameof(videoStream));

            if (request.FrameIntervalSeconds <= 0 || request.FrameIntervalSeconds > 60)
                throw new ArgumentException("Frame interval must be between 1.0 and 60.0 seconds.");

            if (request.MaxFrames <= 0 || request.MaxFrames > 200)
                throw new ArgumentException("Max frames must be between 1 and 200.");

            if (!request.Transcribe && !request.AnalyzeVisuals)
                throw new ArgumentException("At least one of transcribe or analyze_visuals must be true.");

            if (!string.IsNullOrWhiteSpace(request.Language) && request.Language.Length > 10)
                throw new ArgumentException("Language code is invalid.");

            try
            {
                using var content = new MultipartFormDataContent();

                var streamContent = new StreamContent(videoStream);
                var contentType = DetermineContentType(fileName);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                var uploadFileName = fileName ?? "video.mp4";
                content.Add(streamContent, "video", uploadFileName);

                content.Add(new StringContent(request.FrameIntervalSeconds.ToString()), "frame_interval_seconds");
                content.Add(new StringContent(request.MaxFrames.ToString()), "max_frames");
                content.Add(new StringContent(request.Transcribe.ToString().ToLower()), "transcribe");
                content.Add(new StringContent(request.AnalyzeVisuals.ToString().ToLower()), "analyze_visuals");
                content.Add(new StringContent(request.IncludeTimestamps.ToString().ToLower()), "include_timestamps");
                content.Add(new StringContent(request.SummaryFormat.ToString().ToLower()), "summary_format");

                if (!string.IsNullOrWhiteSpace(request.Language))
                {
                    content.Add(new StringContent(request.Language), "language");
                }

                _logger.LogDebug("Sending video analysis request: FrameInterval={FrameInterval}s, MaxFrames={MaxFrames}, Transcribe={Transcribe}, AnalyzeVisuals={AnalyzeVisuals}",
                    request.FrameIntervalSeconds, request.MaxFrames, request.Transcribe, request.AnalyzeVisuals);

                var url = _settings.Video.Urls.Analyze;
                var response = await _httpClient.PostAsync(url, content, ct);

                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<VideoAnalysisResponse>(ct);

                if (result == null)
                {
                    _logger.LogError("Video Analysis API returned null response");
                    throw new InvalidOperationException("Video Analysis API returned empty response");
                }

                _logger.LogInformation("Video analysis completed successfully: Segments={SegmentCount}",
                    result.Segments?.Count ?? 0);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get video analysis from service");
                throw new ServiceUnavailableException("Video Analysis service is unavailable", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Video Analysis request timed out");
                throw new TimeoutException("Video Analysis service timed out", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during video analysis");
                throw;
            }
        }
        private string DetermineContentType(string? fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "video/mp4";

            var extension = Path.GetExtension(fileName)?.ToLower();
            return extension switch
            {
                ".mp4" => "video/mp4",
                ".avi" => "video/x-msvideo",
                ".mov" => "video/quicktime",
                ".mkv" => "video/x-matroska",
                ".webm" => "video/webm",
                ".flv" => "video/x-flv",
                ".wmv" => "video/x-ms-wmv",
                ".m4v" => "video/x-m4v",
                _ => "video/mp4"
            };
        }
    }
}