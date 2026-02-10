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
using System.Text.RegularExpressions;
using QuestionType = AIEduPlatform.Core.Domain.Enums.QuestionType;

namespace AIEduPlatform.ML.Services.Models;

public class OllamaServiceClient : IOllamaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AIServiceSettings _settings;
    private readonly ILogger<OllamaServiceClient> _logger;

    /// <summary>
    /// Regex to strip markdown code fences that LLMs often wrap JSON responses in.
    /// Matches ```json ... ``` or ``` ... ```
    /// </summary>
    private static readonly Regex MarkdownFenceRegex = new(
        @"^```(?:json)?\s*\n?(.*?)\n?\s*```$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    public OllamaServiceClient(
        HttpClient httpClient,
        IOptions<AIServiceSettings> settings,
        ILogger<OllamaServiceClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    #region Core Chat Methods

    /// <summary>
    /// Sends a chat request to Ollama /api/chat with system + user messages.
    /// </summary>
    public async Task<OllamaChatResponse> ChatAsync(
        PromptResult prompt,
        CancellationToken ct = default)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        try
        {
            var url = _settings.Ollama.Urls.Chat;
            var request = BuildChatRequest(prompt, stream: false);

            var response = await _httpClient.PostAsJsonAsync(url, request, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<OllamaChatResponse>(ct);

            if (result == null)
            {
                _logger.LogError("Ollama /api/chat returned null response");
                throw new InvalidOperationException("Ollama API returned empty response");
            }

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get response from Ollama chat service");
            throw new ServiceUnavailableException("Ollama service is unavailable", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Ollama chat request timed out");
            throw new TimeoutException("Ollama service timed out", ex);
        }
    }

    /// <summary>
    /// Streams a chat response from Ollama /api/chat.
    /// </summary>
    public async IAsyncEnumerable<OllamaChatStreamChunk> ChatStreamAsync(
        PromptResult prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        var request = BuildChatRequest(prompt, stream: true);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _settings.Ollama.Urls.Chat)
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
            _logger.LogError(ex, "Failed to establish streaming connection with Ollama chat");
            throw new ServiceUnavailableException("Ollama service is unavailable", ex);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null && !ct.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            OllamaChatStreamChunk? chunk;

            try
            {
                chunk = JsonSerializer.Deserialize<OllamaChatStreamChunk>(line);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize chat stream chunk: {Line}", line);
                continue;
            }

            if (chunk is null)
                continue;

            yield return chunk;

            if (chunk.Done)
                yield break;
        }
    }

    #endregion

    #region Legacy Generate Methods (kept for backward compatibility)

    public async Task<OllamaGenerateResponse> GenerateAsync(
        string prompt,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        try
        {
            var url = _settings.Ollama.Urls.Generate;
            var request = BuildGenerateRequest(prompt, stream: false);

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

        var request = BuildGenerateRequest(prompt, stream: true);

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

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null && !ct.IsCancellationRequested)
        {
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

    #endregion

    #region Study Studio Features

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

        var prompt = PromptBuilder.BuildStudyChatMessages(contextChunks, userQuestion, conversationHistory);
        var chatResponse = await ChatAsync(prompt, ct);

        return new ChatResponse
        {
            Response = chatResponse.Message.Content,
            PromptTokens = chatResponse.PromptEvalCount ?? 0,
            ResponseTokens = chatResponse.EvalCount ?? 0,
            TotalTokens = (chatResponse.PromptEvalCount ?? 0) + (chatResponse.EvalCount ?? 0),
            Model = chatResponse.Model
        };
    }

    public async IAsyncEnumerable<OllamaChatStreamChunk> GenerateStreamStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<OllamaMessage>? conversationHistory = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

        var prompt = PromptBuilder.BuildStudyChatMessages(contextChunks, userQuestion, conversationHistory);

        await foreach (var chunk in ChatStreamAsync(prompt, ct))
        {
            if (!string.IsNullOrEmpty(chunk.Message?.Content))
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

        var prompt = PromptBuilder.BuildFlashCardMessages(contextChunks, topic, numberOfCards);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<List<Flashcard>>(chatResponse.Message.Content, "flashcards");
    }

    public async Task<TeacherStudentDialogue> GenerateTeacherStudentDialogueAsync(
        List<ContextChunk> contextChunks,
        string? topic = null,
        string audienceLevel = "intermediate",
        int numberOfExchanges = 5,
        string dialogueLength = "medium",
        bool includeExamples = true,
        bool includeSummary = true,
        string teachingStyle = "interactive",
        List<string>? focusConcepts = null,
        CancellationToken ct = default)
    {
        if (contextChunks == null || !contextChunks.Any())
            throw new ArgumentException("Context chunks cannot be null or empty.", nameof(contextChunks));

        if (numberOfExchanges <= 0)
            throw new ArgumentException("Number of exchanges must be greater than 0.", nameof(numberOfExchanges));

        // Validate audience level
        var validAudienceLevels = new[] { "beginner", "intermediate", "advanced" };
        if (!validAudienceLevels.Contains(audienceLevel.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid audience level '{Level}', defaulting to 'intermediate'", audienceLevel);
            audienceLevel = "intermediate";
        }

        // Validate dialogue length
        var validLengths = new[] { "short", "medium", "long" };
        if (!validLengths.Contains(dialogueLength.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid dialogue length '{Length}', defaulting to 'medium'", dialogueLength);
            dialogueLength = "medium";
        }

        // Validate teaching style
        var validStyles = new[] { "socratic", "explanatory", "interactive" };
        if (!validStyles.Contains(teachingStyle.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid teaching style '{Style}', defaulting to 'interactive'", teachingStyle);
            teachingStyle = "interactive";
        }

        _logger.LogInformation(
            "Generating teacher-student dialogue: Topic='{Topic}', AudienceLevel={Level}, " +
            "Exchanges={Exchanges}, Length={Length}, Style={Style}",
            topic ?? "auto", audienceLevel, numberOfExchanges, dialogueLength, teachingStyle);

        var prompt = PromptBuilder.BuildTeacherStudentDialogueMessages(
            contextChunks,
            topic,
            audienceLevel,
            numberOfExchanges,
            dialogueLength,
            includeExamples,
            includeSummary,
            teachingStyle,
            focusConcepts);

        var chatResponse = await ChatAsync(prompt, ct);

        var dialogue = DeserializeResponse<TeacherStudentDialogue>(chatResponse.Message.Content, "teacher-student dialogue");

        // Validate the dialogue has proper speaker tags
        if (dialogue.Turns != null)
        {
            foreach (var turn in dialogue.Turns)
            {
                // Normalize speaker names to uppercase
                turn.Speaker = turn.Speaker?.ToUpperInvariant() switch
                {
                    "TEACHER" => "TEACHER",
                    "STUDENT" => "STUDENT",
                    _ => turn.Speaker?.ToUpperInvariant() ?? "UNKNOWN"
                };

                // Warn if speaker is not recognized
                if (turn.Speaker != "TEACHER" && turn.Speaker != "STUDENT")
                {
                    _logger.LogWarning(
                        "Unexpected speaker '{Speaker}' in dialogue turn. Expected 'TEACHER' or 'STUDENT'",
                        turn.Speaker);
                }
            }
        }

        _logger.LogInformation(
            "Generated teacher-student dialogue: Topic='{Topic}', Turns={TurnCount}, " +
            "WordCount={WordCount}, EstimatedDuration={Duration}s",
            dialogue.Topic, dialogue.Turns.Count, dialogue.Summary.Length, dialogue.EstimatedDurationSeconds);

        return dialogue;
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

        var prompt = PromptBuilder.BuildMindMapMessages(contextChunks, centralTopic, maxDepth);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<MindMapNode>(chatResponse.Message.Content, "mind map");
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

        var prompt = PromptBuilder.BuildQuizMessages(contextChunks, topic, numberOfQuestions, difficulty, questionTypes);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<List<QuizQuestion>>(chatResponse.Message.Content, "quiz");
    }

    #endregion

    #region Content Processing

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

        var prompt = PromptBuilder.BuildSummarizationMessages(contextChunks, summaryLength, includeKeyPoints);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<Summary>(chatResponse.Message.Content, "summary");
    }

    public IAsyncEnumerable<Summary> GenerateStreamSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Teacher Features

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

        var prompt = PromptBuilder.BuildEssayGradingMessages(contextChunks, questionText, maxPoints, modelAnswer, studentAnswer);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<EssayGrade>(chatResponse.Message.Content, "essay grade");
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

        var prompt = PromptBuilder.BuildQuestionGenerationMessages(contextChunks, numberOfQuestions, difficulty, questionTypes, focusTopics);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<List<ExamQuestion>>(chatResponse.Message.Content, "exam questions");
    }

    #endregion

    #region Model Management

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

    #endregion

    #region Private Helpers

    /// <summary>
    /// Builds an OllamaChatRequest with system + user messages for /api/chat.
    /// If the PromptResult contains ConversationHistory, those messages are inserted
    /// as proper alternating user/assistant turns between the system message and
    /// the final user message, giving the LLM native multi-turn context.
    /// </summary>
    private OllamaChatRequest BuildChatRequest(PromptResult prompt, bool stream)
    {
        if (_settings.Ollama == null)
            throw new InvalidOperationException("Ollama settings are not configured");

        var messages = new List<OllamaMessage>
        {
            new OllamaMessage { Role = "system", Content = prompt.SystemMessage }
        };

        // Insert conversation history as proper alternating messages
        if (prompt.ConversationHistory != null && prompt.ConversationHistory.Any())
        {
            foreach (var historyMsg in prompt.ConversationHistory)
            {
                // Only include user/assistant messages (skip any system messages in history)
                if (historyMsg.Role == "user" || historyMsg.Role == "assistant")
                {
                    messages.Add(new OllamaMessage
                    {
                        Role = historyMsg.Role,
                        Content = historyMsg.Content
                    });
                }
            }
        }

        // Final user message with context + current question
        messages.Add(new OllamaMessage { Role = "user", Content = prompt.UserMessage });

        return new OllamaChatRequest
        {
            Model = _settings.Ollama.Model,
            Messages = messages,
            Stream = stream,
            KeepAlive = _settings.Ollama.KeepAlive,
            Options = _settings.Ollama.Options
        };
    }

    /// <summary>
    /// Builds an OllamaRequest for legacy /api/generate endpoint.
    /// </summary>
    private OllamaRequest BuildGenerateRequest(string prompt, bool stream)
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

    /// <summary>
    /// Strips markdown code fences (```json ... ```) that LLMs often wrap responses in,
    /// then deserializes the JSON.
    /// </summary>
    private T DeserializeResponse<T>(string jsonResponse, string contentType)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            _logger.LogError("Received empty response when deserializing {ContentType}", contentType);
            throw new InvalidOperationException($"Ollama returned empty response for {contentType}");
        }

        // Strip markdown code fences that LLMs commonly wrap JSON in
        var cleaned = jsonResponse.Trim();
        var fenceMatch = MarkdownFenceRegex.Match(cleaned);
        if (fenceMatch.Success)
        {
            cleaned = fenceMatch.Groups[1].Value.Trim();
            _logger.LogDebug("Stripped markdown fence from {ContentType} response", contentType);
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(cleaned);

            if (result == null)
            {
                _logger.LogError("Failed to deserialize {ContentType}. Response: {Response}",
                    contentType, cleaned);
                throw new InvalidOperationException($"Failed to parse {contentType} from Ollama response");
            }

            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for {ContentType}. Response: {Response}",
                contentType, cleaned);
            throw new InvalidOperationException($"Invalid JSON format for {contentType}: {ex.Message}", ex);
        }
    }

    #endregion
}
