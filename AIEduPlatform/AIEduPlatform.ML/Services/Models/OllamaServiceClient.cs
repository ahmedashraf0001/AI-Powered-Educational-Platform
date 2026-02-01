using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Common;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Responses;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.Embedding;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuestionType = AIEduPlatform.Core.Domain.Enums.QuestionType;

namespace AIEduPlatform.ML.Services.Models;

public class OllamaServiceClient : IOllamaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AIServiceSettings _settings;
    private readonly ILogger<OllamaServiceClient> _logger;

    public OllamaServiceClient(
        HttpClient httpClient,
        IOptions<AIServiceSettings> settings,
        ILogger<OllamaServiceClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<OllamaGenerateResponse> GenerateAsync(
        string prompt,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        try
        {
            var url = _settings.Ollama.Urls.Generate;
            var request = BuildRequest(prompt, stream: false);

            var response = await _httpClient.PostAsJsonAsync(url, request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>(ct);

            if (result == null)
            {
                _logger.LogError("Ollama API returned null response");
                throw new InvalidOperationException("Ollama API returned empty response");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get response from Ollama service");
            throw new ServiceUnavailableException("Ollama service is unavailable", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Ollama request timed out");
            throw new TimeoutException("Ollama service timed out", ex);
        }
    }

    public async IAsyncEnumerable<OllamaGenerateStreamChunk> GenerateStreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        var request = BuildRequest(prompt, stream: true);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.Ollama.Urls.Generate)
        {
            Content = JsonContent.Create(request)
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(
                httpRequest,
                HttpCompletionOption.ResponseHeadersRead,
                ct);

            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to establish streaming connection with Ollama");
            throw new ServiceUnavailableException("Ollama service is unavailable", ex);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream && !ct.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(ct);

            if (string.IsNullOrWhiteSpace(line))
                continue;

            OllamaGenerateStreamChunk? chunk;

            try
            {
                chunk = JsonSerializer.Deserialize<OllamaGenerateStreamChunk>(line);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize stream chunk: {Line}", line);
                continue;
            }

            if (chunk is null)
                continue;

            yield return chunk;

            if (chunk.Done)
                yield break;
        }
    }

    public async Task<ChatResponse> GenerateStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<OllamaMessage>? conversationHistory = null,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

        var prompt = PromptBuilder.BuildStudyChatPrompt(contextChunks, userQuestion, conversationHistory);
        var response = await GenerateAsync(prompt, ct);

        return new ChatResponse
        {
            Response = response.Response,
            PromptTokens = response.PromptEvalCount ?? 0,
            ResponseTokens = response.EvalCount ?? 0,
            TotalTokens = (response.PromptEvalCount ?? 0) + (response.EvalCount ?? 0),
            Model = response.Model
        };
    }

    public async IAsyncEnumerable<OllamaGenerateStreamChunk> GenerateStreamStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<OllamaMessage>? conversationHistory = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

        var prompt = PromptBuilder.BuildStudyChatPrompt(contextChunks, userQuestion, conversationHistory);

        await foreach (var chunk in GenerateStreamAsync(prompt, ct))
        {
            if (!string.IsNullOrEmpty(chunk.Response))
                yield return chunk;
        }
    }

    public async Task<List<Flashcard>> GenerateFlashcardsAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfCards = 10,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

        if (numberOfCards <= 0)
            throw new ArgumentException("Number of cards must be greater than 0.", nameof(numberOfCards));

        var prompt = PromptBuilder.BuildFlashCardPrompt(contextChunks, topic, numberOfCards);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<List<Flashcard>>(response.Response, "flashcards");
    }

    public async Task<MindMapNode> GenerateMindMapAsync(
        List<ContextChunk> contextChunks,
        string centralTopic,
        int maxDepth = 3,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(centralTopic))
            throw new ArgumentException("Central topic cannot be null or empty.", nameof(centralTopic));

        if (maxDepth <= 0)
            throw new ArgumentException("Max depth must be greater than 0.", nameof(maxDepth));

        var prompt = PromptBuilder.BuildMindMapPrompt(contextChunks, centralTopic, maxDepth);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<MindMapNode>(response.Response, "mind map");
    }

    public async Task<List<QuizQuestion>> GenerateQuizAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfQuestions,
        string difficulty,
        List<QuestionType> questionTypes,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

        if (numberOfQuestions <= 0)
            throw new ArgumentException("Number of questions must be greater than 0.", nameof(numberOfQuestions));

        if (string.IsNullOrWhiteSpace(difficulty))
            throw new ArgumentException("Difficulty cannot be null or empty.", nameof(difficulty));

        if (questionTypes == null || !questionTypes.Any())
            throw new ArgumentException("Question types cannot be null or empty.", nameof(questionTypes));

        var prompt = PromptBuilder.BuildQuizPrompt(contextChunks, topic, numberOfQuestions, difficulty, questionTypes);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<List<QuizQuestion>>(response.Response, "quiz");
    }

    public async Task<Summary> GenerateSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (summaryLength <= 0)
            throw new ArgumentException("Summary length must be greater than 0.", nameof(summaryLength));

        var prompt = PromptBuilder.BuildSummarizationPrompt(contextChunks, summaryLength, includeKeyPoints);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<Summary>(response.Response, "summary");
    }

    public IAsyncEnumerable<Summary> GenerateStreamSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async Task<EssayGrade> GradeEssayAsync(
        List<ContextChunk> contextChunks,
        string questionText,
        int maxPoints,
        string modelAnswer,
        string studentAnswer,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(questionText))
            throw new ArgumentException("Question text cannot be null or empty.", nameof(questionText));

        if (maxPoints <= 0)
            throw new ArgumentException("Max points must be greater than 0.", nameof(maxPoints));

        if (string.IsNullOrWhiteSpace(studentAnswer))
            throw new ArgumentException("Student answer cannot be null or empty.", nameof(studentAnswer));


        var prompt = PromptBuilder.BuildEssayGradingPrompt(
            contextChunks, questionText, maxPoints, modelAnswer, studentAnswer);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<EssayGrade>(response.Response, "essay grade");
    }

    public async Task<List<ExamQuestion>> GenerateExamQuestionsAsync(
        List<ContextChunk> contextChunks,
        int numberOfQuestions,
        string difficulty,
        List<string> questionTypes,
        List<string>? focusTopics = null,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (numberOfQuestions <= 0)
            throw new ArgumentException("Number of questions must be greater than 0.", nameof(numberOfQuestions));

        if (string.IsNullOrWhiteSpace(difficulty))
            throw new ArgumentException("Difficulty cannot be null or empty.", nameof(difficulty));

        if (questionTypes == null || !questionTypes.Any())
            throw new ArgumentException("Question types cannot be null or empty.", nameof(questionTypes));

        var prompt = PromptBuilder.BuildQuestionGenerationPrompt(
            contextChunks, numberOfQuestions, difficulty, questionTypes, focusTopics);
        var response = await GenerateAsync(prompt, ct);

        return DeserializeResponse<List<ExamQuestion>>(response.Response, "exam questions");
    }

    public async Task<bool> IsModelAvailableAsync(string model, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(model))
            throw new ArgumentException("Model name cannot be null or empty.", nameof(model));

        try
        {
            var models = await GetAvailableModelsAsync(ct);
            return models?.Any(m => m.Equals(model, StringComparison.OrdinalIgnoreCase)) ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check model availability for {Model}", model);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableModelsAsync(CancellationToken ct = default)
    {
        try
        {
            var url = $"{_settings.BaseUrls.OllamaService}/api/tags";

            var response = await _httpClient.GetFromJsonAsync<OllamaModelsResponse>(url, ct);

            if (response?.Models == null || !response.Models.Any())
            {
                _logger.LogWarning("No models returned from Ollama API");
                return new List<string>();
            }

            return response.Models.Select(m => m.Name).ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch available models from Ollama");
            throw new ServiceUnavailableException("Failed to fetch available models from Ollama service", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize models response from Ollama");
            throw new InvalidOperationException("Invalid response format from Ollama models API", ex);
        }
    }

    private OllamaRequest BuildRequest(string prompt, bool stream)
    {
        if (_settings.Ollama == null)
            throw new InvalidOperationException("Ollama settings are not configured");

        return new OllamaRequest
        {
            Model = _settings.Ollama.Model,
            Prompt = prompt,
            Stream = stream,
            KeepAlive = _settings.Ollama.KeepAlive,
            Options = _settings.Ollama.Options
        };
    }

    private T DeserializeResponse<T>(string jsonResponse, string contentType)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            _logger.LogError("Received empty response when deserializing {ContentType}", contentType);
            throw new InvalidOperationException($"Ollama returned empty response for {contentType}");
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(jsonResponse);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize {ContentType}. Response: {Response}",
                    contentType, jsonResponse);
                throw new InvalidOperationException($"Failed to parse {contentType} from Ollama response");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for {ContentType}. Response: {Response}",
                contentType, jsonResponse);
            throw new InvalidOperationException($"Invalid JSON format for {contentType}: {ex.Message}", ex);
        }
    }
}
