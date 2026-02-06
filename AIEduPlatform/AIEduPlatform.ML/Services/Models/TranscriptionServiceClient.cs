using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.ML.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;

namespace AIEduPlatform.ML.Services.Models
{
    public class TranscriptionServiceClient : ITranscriptionService
    {
        private readonly HttpClient _httpClient;
        private readonly AIServiceSettings _settings;
        private readonly ILogger<TranscriptionServiceClient> _logger;

        public TranscriptionServiceClient(HttpClient httpClient, AIServiceSettings settings, ILogger<TranscriptionServiceClient> logger)
        {
            _httpClient = httpClient;
            _settings = settings;
            _logger = logger;
        }
        private async Task<TOutput> PostRequestAsync<TInput, TOutput>(
             string url,
             TInput request,
             CancellationToken ct = default,
             string contentType = "application/json")
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url), "URL is not configured");
            if (request == null)
                throw new ArgumentException("Request cannot be null.", nameof(request));

            try
            {
                HttpContent content;

                if (request is HttpContent httpContent)
                {
                    content = httpContent;
                }
                else if (contentType == "application/json")
                {
                    content = JsonContent.Create(request);
                }
                else
                {
                    throw new NotSupportedException($"Content type '{contentType}' is not supported for non-HttpContent requests");
                }

                var response = await _httpClient.PostAsync(url, content, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TOutput>(ct).ConfigureAwait(false);

                if (result == null)
                {
                    _logger.LogError("Transcription API returned null response");
                    throw new InvalidOperationException("Transcription API returned empty response");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get response from Transcription service");
                throw new ServiceUnavailableException("Transcription service is unavailable", ex);
            }
            catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation("Transcription request was cancelled");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Transcription request timed out");
                throw new TimeoutException("Transcription service timed out", ex);
            }
        }
        private async Task<TOutput> GetRequestAsync<TOutput>(
             string url,
             CancellationToken ct = default)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url), "Voices URL is not configured");
            try
            {
                var response = await _httpClient.GetAsync(url, ct).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<TOutput>(ct).ConfigureAwait(false);

                if (result == null)
                {
                    _logger.LogError("Transcription API returned null response");
                    throw new InvalidOperationException("Transcription API returned empty response");
                }

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to get response from Transcription service");
                throw new ServiceUnavailableException("Transcription service is unavailable", ex);
            }
            catch (OperationCanceledException ex) when (ct.IsCancellationRequested)
            {
                _logger.LogInformation("Transcription request was cancelled");
                throw;
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Transcription request timed out");
                throw new TimeoutException("Transcription service timed out", ex);
            }
        }
        public async Task<DialogueAudioResult> GenerateDialogueAudioAsync(
            TeacherStudentDialogue dialogue,
            DefaultVoiceConfigResult? config = null,
            CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.GenerateDialogue;
            if (url == null)
                throw new ArgumentNullException(nameof(url), "GenerateDialogue URL is not configured");

            if (dialogue == null)
                throw new ArgumentNullException(nameof(dialogue));

            var turns = dialogue.Turns
                .Select(t => new DialogueTurn(
                    Speaker: t.Speaker,
                    Text: t.Content))
                .ToList();

            var voiceConfig = config != null
                ? new DialogueVoiceConfig(
                    TeacherVoiceId: config.TeacherVoiceId,
                    StudentVoiceId: config.StudentVoiceId,
                    TeacherSpeed: config.TeacherSpeed,
                    StudentSpeed: config.StudentSpeed)
                : new DialogueVoiceConfig();

            var request = new DialogueRequest(
                Turns: turns,
                Topic: dialogue.Topic,
                VoiceConfig: voiceConfig);

            var response = await PostRequestAsync<DialogueRequest, DialogueAudioResult>(url, request, ct);

            if (response.Success && !string.IsNullOrEmpty(response.audio_base64))
            {
                dialogue.EstimatedDurationSeconds = (int)Math.Ceiling(response.duration_seconds);
            }

            return response;
        }

        public async Task<IReadOnlyList<VoiceInfo>> GetAvailableVoicesAsync(CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.Voices;
            var response = await GetRequestAsync<IReadOnlyList<VoiceInfo>>(url, ct);
            return response;    
        }

        public async Task<DefaultVoiceConfigResult> GetDefaultVoiceConfigAsync(CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.DefaultVoiceConfig;
            var response = await GetRequestAsync<DefaultVoiceConfigResult>(url, ct);
            return response;
        }

        public async Task<SupportedFormatsResult> GetSupportedFormatsAsync(CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.SupportedFormats;
            var response = await GetRequestAsync<SupportedFormatsResult>(url, ct);
            return response;
        }

        public async Task<SupportedLanguagesResult> GetSupportedInputLanguagesAsync(CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.DefaultVoiceConfig;
            var response = await GetRequestAsync<SupportedLanguagesResult>(url, ct);
            return response;
        }

        public async Task<IReadOnlyList<VoicePreview>> GetVoicePreviewsAsync(
            string? voiceId = null,
            string? sampleText = null,
            string format = "mp3",
            int sampleRate = 22050,
            CancellationToken ct = default)
        {
            var baseUrl = _settings.Transcription.Urls.VoicePreviews;
            if (baseUrl == null)
                throw new ArgumentNullException(nameof(baseUrl), "Voice Previews URL is not configured");

            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(voiceId))
                queryParams.Add($"voice_id={Uri.EscapeDataString(voiceId)}");

            if (!string.IsNullOrEmpty(sampleText))
                queryParams.Add($"sample_text={Uri.EscapeDataString(sampleText)}");

            queryParams.Add($"format={Uri.EscapeDataString(format)}");
            queryParams.Add($"sample_rate={sampleRate}");

            var url = queryParams.Count > 0
                ? $"{baseUrl}?{string.Join("&", queryParams)}"
                : baseUrl;

            var response = await GetRequestAsync<IReadOnlyList<VoicePreview>>(url, ct);
            return response;
        }

        public async Task<SynthesisResult> SynthesizeTextAsync(SynthesizeRequest request, CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.SynthesizeText;
            var response = await PostRequestAsync<SynthesizeRequest, SynthesisResult>(url, request, ct);
            return response;
        }

        public async Task<BatchTranscriptionResult> TranscribeBatchAsync(BatchTranscriptionRequest request, CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.TranscribeBatch;
            var response = await PostRequestAsync<BatchTranscriptionRequest, BatchTranscriptionResult>(url, request, ct);
            return response;
        }

        public async Task<SpeechToTextResult> TranscribeFileAsync(
            Stream audioStream,
            string fileName,
            string fileType, //wav
            string? sourceLanguage = null,
            string task = "translate",
            bool includeTimestamps = true,
            bool includeMetadata = false,
            CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.TranscribeFile;
            if (url == null)
                throw new ArgumentNullException(nameof(url), "Transcribe File URL is not configured");

            if (audioStream == null)
                throw new ArgumentNullException(nameof(audioStream));

            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            var formData = new MultipartFormDataContent();

            var fileContent = new StreamContent(audioStream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue($"audio/" + fileType); 
            formData.Add(fileContent, "file", fileName);

            if (!string.IsNullOrEmpty(sourceLanguage))
                formData.Add(new StringContent(sourceLanguage), "language");

            formData.Add(new StringContent(task), "task");
            formData.Add(new StringContent(includeTimestamps.ToString().ToLower()), "include_timestamps");
            formData.Add(new StringContent(includeMetadata.ToString().ToLower()), "include_metadata");

            var response = await PostRequestAsync<HttpContent, SpeechToTextResult>(url, formData, ct);
            return response;
        }

        public async Task<SpeechToTextResult> TranscribeToEnglishAsync(TranscribeAudioRequest request, CancellationToken ct = default)
        {
            var url = _settings.Transcription.Urls.TranscribeBase64;
            var response = await PostRequestAsync<TranscribeAudioRequest, SpeechToTextResult>(url, request, ct);
            return response;
        }
    }
}
