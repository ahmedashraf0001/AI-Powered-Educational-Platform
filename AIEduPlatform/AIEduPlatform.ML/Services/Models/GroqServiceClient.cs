using AiEduPlatform.SharedKernal.exceptions;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Groq;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Responses;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.ML.Configurations;
using AIEduPlatform.ML.Prompts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using QuestionType = AIEduPlatform.Core.Domain.Enums.QuestionType;

namespace AIEduPlatform.ML.Services.Models;

/// <summary>
/// LLM service client that talks to Groq's OpenAI-compatible chat completions API.
/// Implements <see cref="IOllamaServiceClient"/> so it can be used as a drop-in replacement
/// for Ollama throughout the application. Response types are mapped to the existing Ollama DTOs.
/// </summary>
public class GroqServiceClient : IOllamaServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly AIServiceSettings _settings;
    private readonly ILogger<GroqServiceClient> _logger;

    private static readonly Regex MarkdownFenceRegex = new(
        @"^```(?:json)?\s*\n?(.*?)\n?\s*```$",
        RegexOptions.Singleline | RegexOptions.Compiled);

    public GroqServiceClient(
        HttpClient httpClient,
        IOptions<AIServiceSettings> settings,
        ILogger<GroqServiceClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    #region Core Chat Methods

    /// <summary>
    /// Sends a chat request to Groq and maps the response to <see cref="OllamaChatResponse"/>.
    /// </summary>
    public async Task<OllamaChatResponse> ChatAsync(
        PromptResult prompt,
        CancellationToken ct = default)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        try
        {
            var url = _settings.Groq.Urls.Chat;
            var request = BuildChatRequest(prompt, stream: false);

            var response = await SendGroqChatWithFallbackAsync(url, request, stream: false, ct);

            var groqResponse = await response.Content.ReadFromJsonAsync<GroqChatResponse>(ct);

            if (groqResponse == null)
            {
                _logger.LogError("Groq API returned null response");
                throw new InvalidOperationException("Groq API returned empty response");
            }

            return MapToOllamaChatResponse(groqResponse);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to get response from Groq chat service");
            throw new ServiceUnavailableException("Groq service is unavailable", ex);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Groq chat request timed out");
            throw new TimeoutException("Groq service timed out", ex);
        }
    }

    /// <summary>
    /// Streams a chat response from Groq using SSE format.
    /// Maps each chunk to <see cref="OllamaChatStreamChunk"/>.
    /// </summary>
    private async IAsyncEnumerable<OllamaChatStreamChunk> ChatStreamAsync(
        PromptResult prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (prompt == null)
            throw new ArgumentNullException(nameof(prompt));

        var request = BuildChatRequest(prompt, stream: true);

        HttpResponseMessage response;
        try
        {
            response = await SendGroqChatWithFallbackAsync(_settings.Groq.Urls.Chat, request, stream: true, ct);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to establish streaming connection with Groq");
            throw new ServiceUnavailableException("Groq service is unavailable", ex);
        }

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        string? line;
        while ((line = await reader.ReadLineAsync(ct)) is not null && !ct.IsCancellationRequested)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Groq uses SSE format: "data: {json}" or "data: [DONE]"
            if (!line.StartsWith("data: "))
                continue;

            var data = line["data: ".Length..];

            if (data == "[DONE]")
            {
                yield return new OllamaChatStreamChunk
                {
                    Model = _settings.Groq.Model,
                    CreatedAt = DateTime.UtcNow,
                    Message = new OllamaMessage { Role = "assistant", Content = "" },
                    Done = true
                };
                yield break;
            }

            GroqStreamChunk? chunk;
            try
            {
                chunk = JsonSerializer.Deserialize<GroqStreamChunk>(data);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize Groq stream chunk: {Line}", data);
                continue;
            }

            if (chunk is null)
                continue;

            var content = chunk.Choices.FirstOrDefault()?.Delta?.Content ?? "";

            yield return new OllamaChatStreamChunk
            {
                Model = chunk.Model,
                CreatedAt = DateTime.UtcNow,
                Message = new OllamaMessage { Role = "assistant", Content = content },
                Done = false
            };
        }
    }

    private async Task<HttpResponseMessage> SendGroqChatWithFallbackAsync(
        string url,
        GroqChatRequest request,
        bool stream,
        CancellationToken ct)
    {
        var response = await SendGroqChatRequestAsync(url, request, stream, ct);

        if (response.IsSuccessStatusCode)
            return response;

        var errorBody = await SafeReadBodyAsync(response, ct);

        // Most common Groq 404 is model-not-found or model-no-longer-available.
        // Retry once with first available model from account.
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            var fallbackModel = await ResolveFallbackModelAsync(request.Model, ct);

            if (!string.IsNullOrWhiteSpace(fallbackModel))
            {
                _logger.LogWarning(
                    "Groq returned 404 for model '{Model}'. Retrying with fallback model '{FallbackModel}'. Body: {Body}",
                    request.Model,
                    fallbackModel,
                    errorBody);

                response.Dispose();

                var retriedRequest = request with { Model = fallbackModel };
                response = await SendGroqChatRequestAsync(url, retriedRequest, stream, ct);

                if (response.IsSuccessStatusCode)
                    return response;

                errorBody = await SafeReadBodyAsync(response, ct);
            }
        }

        _logger.LogError(
            "Groq request failed. Status={StatusCode} {ReasonPhrase}, Url={Url}, Model={Model}, Body={Body}",
            (int)response.StatusCode,
            response.ReasonPhrase,
            url,
            request.Model,
            errorBody);

        throw new InvalidOperationException(
            $"Groq request failed ({(int)response.StatusCode} {response.ReasonPhrase}). " +
            $"Model: '{request.Model}'. Url: '{url}'. Response: {errorBody}");
    }

    private async Task<HttpResponseMessage> SendGroqChatRequestAsync(
        string url,
        GroqChatRequest request,
        bool stream,
        CancellationToken ct)
    {
        if (!stream)
            return await _httpClient.PostAsJsonAsync(url, request, ct);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = JsonContent.Create(request)
        };

        return await _httpClient.SendAsync(
            httpRequest,
            HttpCompletionOption.ResponseHeadersRead,
            ct);
    }

    // Preferred chat-capable models in priority order; first available wins.
    private static readonly string[] PreferredModels =
    [
        "llama-3.3-70b-versatile",
        "llama-3.1-70b-versatile",
        "llama3-70b-8192",
        "llama-3.1-8b-instant",
        "llama3-8b-8192",
        "mixtral-8x7b-32768",
        "gemma2-9b-it",
    ];

    private async Task<string?> ResolveFallbackModelAsync(string currentModel, CancellationToken ct)
    {
        try
        {
            var models = await GetAvailableModelsAsync(ct);

            _logger.LogInformation("Available Groq models: {Models}", string.Join(", ", models));

            // First: try preferred models that aren't the one that just failed
            foreach (var preferred in PreferredModels)
            {
                if (models.Any(m => string.Equals(m, preferred, StringComparison.OrdinalIgnoreCase))
                    && !string.Equals(preferred, currentModel, StringComparison.OrdinalIgnoreCase))
                {
                    return preferred;
                }
            }

            // Fallback: any model that isn't the failing one
            return models.FirstOrDefault(m => !string.Equals(m, currentModel, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to resolve fallback Groq model");
            return null;
        }
    }

    private static async Task<string> SafeReadBodyAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(body))
                return "<empty>";

            return body.Length > 1200 ? body[..1200] : body;
        }
        catch
        {
            return "<unavailable>";
        }
    }

    #endregion

    #region Legacy Generate Methods

    public async Task<OllamaGenerateResponse> GenerateAsync(
        string prompt,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        // Use chat completions API with the prompt as a user message
        var promptResult = new PromptResult
        {
            SystemMessage = "You are a helpful assistant.",
            UserMessage = prompt
        };

        var chatResponse = await ChatAsync(promptResult, ct);

        return new OllamaGenerateResponse
        {
            Model = chatResponse.Model,
            CreatedAt = chatResponse.CreatedAt,
            Response = chatResponse.Message.Content,
            Done = true,
            DoneReason = chatResponse.DoneReason,
            PromptEvalCount = chatResponse.PromptEvalCount,
            EvalCount = chatResponse.EvalCount
        };
    }

    public async IAsyncEnumerable<OllamaGenerateStreamChunk> GenerateStreamAsync(
        string prompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            throw new ArgumentException("Prompt cannot be null or empty.", nameof(prompt));

        var promptResult = new PromptResult
        {
            SystemMessage = "You are a helpful assistant.",
            UserMessage = prompt
        };

        await foreach (var chunk in ChatStreamAsync(promptResult, ct))
        {
            yield return new OllamaGenerateStreamChunk
            {
                Model = chunk.Model,
                CreatedAt = chunk.CreatedAt,
                Response = chunk.Message?.Content ?? "",
                Done = chunk.Done
            };
        }
    }

    #endregion

    #region Study Studio Features

    public async Task<ChatResponse> GenerateStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        string intent,
        List<Guid>? targetMaterialIds = null,
        List<OllamaMessage>? conversationHistory = null,
        CancellationToken ct = default)
    {
        contextChunks ??= new List<ContextChunk>();

        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

        var prompt = PromptBuilder.BuildStudyChatMessages(contextChunks, userQuestion, intent, conversationHistory, targetMaterialIds);
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
        string intent,
        List<Guid>? targetMaterialIds = null,
        List<OllamaMessage>? conversationHistory = null,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        contextChunks ??= new List<ContextChunk>();

        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be null or empty.", nameof(userQuestion));

        var prompt = PromptBuilder.BuildStudyChatMessages(contextChunks, userQuestion, intent, conversationHistory, targetMaterialIds);

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

        var validAudienceLevels = new[] { "beginner", "intermediate", "advanced" };
        if (!validAudienceLevels.Contains(audienceLevel.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid audience level '{Level}', defaulting to 'intermediate'", audienceLevel);
            audienceLevel = "intermediate";
        }

        var validLengths = new[] { "short", "medium", "long" };
        if (!validLengths.Contains(dialogueLength.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid dialogue length '{Length}', defaulting to 'medium'", dialogueLength);
            dialogueLength = "medium";
        }

        var validStyles = new[] { "socratic", "explanatory", "interactive" };
        if (!validStyles.Contains(teachingStyle.ToLowerInvariant()))
        {
            _logger.LogWarning("Invalid teaching style '{Style}', defaulting to 'interactive'", teachingStyle);
            teachingStyle = "interactive";
        }

        _logger.LogInformation(
            "Generating teacher-student dialogue via Groq: Topic='{Topic}', AudienceLevel={Level}, " +
            "Exchanges={Exchanges}, Length={Length}, Style={Style}",
            topic ?? "auto", audienceLevel, numberOfExchanges, dialogueLength, teachingStyle);

        var prompt = PromptBuilder.BuildTeacherStudentDialogueMessages(
            contextChunks, topic, audienceLevel, numberOfExchanges,
            dialogueLength, includeExamples, includeSummary, teachingStyle, focusConcepts);

        var chatResponse = await ChatAsync(prompt, ct);

        var dialogue = DeserializeResponse<TeacherStudentDialogue>(chatResponse.Message.Content, "teacher-student dialogue");

        if (dialogue.Turns != null)
        {
            foreach (var turn in dialogue.Turns)
            {
                turn.Speaker = turn.Speaker?.ToUpperInvariant() switch
                {
                    "TEACHER" => "TEACHER",
                    "STUDENT" => "STUDENT",
                    _ => turn.Speaker?.ToUpperInvariant() ?? "UNKNOWN"
                };

                if (turn.Speaker != "TEACHER" && turn.Speaker != "STUDENT")
                {
                    _logger.LogWarning(
                        "Unexpected speaker '{Speaker}' in dialogue turn. Expected 'TEACHER' or 'STUDENT'",
                        turn.Speaker);
                }
            }
        }

        _logger.LogInformation(
            "Generated teacher-student dialogue via Groq: Topic='{Topic}', Turns={TurnCount}, " +
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
            _logger.LogWarning(ex, "Failed to check Groq model availability for {Model}", model);
            return false;
        }
    }

    public async Task<List<string>> GetAvailableModelsAsync(CancellationToken ct = default)
    {
        try
        {
            var url = "/openai/v1/models";
            var response = await _httpClient.GetFromJsonAsync<GroqModelsResponse>(url, ct);

            if (response?.Data == null || !response.Data.Any())
            {
                _logger.LogWarning("No models returned from Groq API");
                return new List<string>();
            }

            return response.Data
                .Where(m => m.Active)
                .Select(m => m.Id)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch available models from Groq");
            throw new ServiceUnavailableException("Failed to fetch available models from Groq service", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize models response from Groq");
            throw new InvalidOperationException("Invalid response format from Groq models API", ex);
        }
    }

    #endregion

    #region Private Helpers

    // ── Token-budget constants for Groq's rate-limited plans ──────────────
    // Groq free/dev plans often cap total request size at ~12 000 tokens.
    // We leave a safety margin so the request never exceeds the limit.
    private const int GroqMaxContextTokens = 32_000;  // safe ceiling for most Groq plans
    private const int TokenSafetyMargin    = 200;      // overhead for JSON framing / role tags
    private const int DefaultMaxCompletionTokens = 8_192; // llama-3.3-70b supports 32,768 output
    private const int MaxHistoryMessages   = 6;        // keep last N turns (3 pairs)

    /// <summary>
    /// Rough token estimate: ~4 chars per token for English text.
    /// Good enough for budgeting; avoids pulling in a real tokenizer.
    /// </summary>
    private static int EstimateTokens(string? text)
        => string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 3.5);

    /// <summary>
    /// Builds a <see cref="GroqChatRequest"/> from a <see cref="PromptResult"/>.
    /// Priority order: 1) Instructions (system) — never trimmed unless extreme
    ///                 2) Content/RAG (user message) — trimmed only as last resort
    ///                 3) Chat history — first to be dropped when budget is tight
    /// </summary>
    private GroqChatRequest BuildChatRequest(PromptResult prompt, bool stream)
    {
        if (_settings.Groq == null)
            throw new InvalidOperationException("Groq settings are not configured");

        var maxCompletion = _settings.Groq.Options?.MaxTokens ?? DefaultMaxCompletionTokens;

        // Total budget for all input messages
        var inputBudget = GroqMaxContextTokens - maxCompletion - TokenSafetyMargin;
        if (inputBudget < 500)
        {
            _logger.LogWarning(
                "Groq input budget is very small ({Budget} tokens). Consider lowering MaxTokens ({MaxTokens})",
                inputBudget, maxCompletion);
            inputBudget = 500;
        }

        // ── PRIORITY 1: Instructions (system prompt) — takes what it needs first ──
        // Only truncated if it alone exceeds 60% of the entire input budget (extreme case).
        var systemContent = prompt.SystemMessage;
        var systemTokens  = EstimateTokens(systemContent);
        var systemCeiling  = (int)(inputBudget * 0.60);
        if (systemTokens > systemCeiling)
        {
            var maxChars = (int)(systemCeiling * 3.5);
            systemContent = systemContent[..Math.Min(maxChars, systemContent.Length)];
            systemTokens  = EstimateTokens(systemContent);
            _logger.LogWarning("Groq system prompt truncated from ~{Original} to ~{Truncated} estimated tokens",
                EstimateTokens(prompt.SystemMessage), systemTokens);
        }

        var budgetAfterSystem = inputBudget - systemTokens;

        // ── PRIORITY 2: Content / RAG context (user message) — uses remaining budget freely ──
        // Only truncated if it exceeds what's left after instructions (minus a small
        // reserve so at least 1-2 history messages can fit if possible).
        var userContent = prompt.UserMessage;
        var userTokens  = EstimateTokens(userContent);
        var historyReserve = Math.Min((int)(budgetAfterSystem * 0.20), 1000); // slightly more room for history
        var userCeiling = budgetAfterSystem - historyReserve;
        if (userCeiling < 200) userCeiling = budgetAfterSystem; // if budget is tiny, skip history reserve

        if (userTokens > userCeiling)
        {
            var maxChars = (int)(userCeiling * 3.5);
            userContent = userContent[..Math.Min(maxChars, userContent.Length)]
                          + "\n\n[Content truncated to fit token budget]";
            userTokens  = EstimateTokens(userContent);
            _logger.LogWarning("Groq user/RAG content truncated from ~{Original} to ~{Truncated} estimated tokens",
                EstimateTokens(prompt.UserMessage), userTokens);
        }

        // ── PRIORITY 3: Chat history — gets whatever is left ──
        var remainingBudget = inputBudget - systemTokens - userTokens;

        var messages = new List<GroqMessage>
        {
            new() { Role = "system", Content = systemContent }
        };

        if (prompt.ConversationHistory != null && prompt.ConversationHistory.Any() && remainingBudget > 80)
        {
            var recentHistory = prompt.ConversationHistory
                .Where(m => m.Role == "user" || m.Role == "assistant")
                .TakeLast(MaxHistoryMessages)
                .ToList();

            // Drop oldest messages until they fit
            while (recentHistory.Count > 0)
            {
                var historyTokens = recentHistory.Sum(m => EstimateTokens(m.Content) + 4);
                if (historyTokens <= remainingBudget)
                    break;
                recentHistory.RemoveAt(0);
            }

            if (recentHistory.Count > 0)
            {
                foreach (var historyMsg in recentHistory)
                {
                    messages.Add(new GroqMessage
                    {
                        Role = historyMsg.Role,
                        Content = historyMsg.Content
                    });
                }

                _logger.LogDebug("Included {Count} history messages (~{Tokens} tokens) in Groq request",
                    recentHistory.Count,
                    recentHistory.Sum(m => EstimateTokens(m.Content)));
            }
            else
            {
                _logger.LogWarning("All conversation history dropped to fit Groq token budget");
            }
        }

        // Final user message always goes last
        messages.Add(new GroqMessage { Role = "user", Content = userContent });

        var totalEstimated = messages.Sum(m => EstimateTokens(m.Content) + 4) + maxCompletion;
        _logger.LogDebug(
            "Groq budget: system ~{SysTokens} + content ~{UserTokens} + history ~{HistTokens} + completion {MaxComp} = ~{Total} (limit {Limit})",
            systemTokens, userTokens,
            messages.Where(m => m.Role != "system").Sum(m => EstimateTokens(m.Content)) - userTokens,
            maxCompletion, totalEstimated, GroqMaxContextTokens);

        return new GroqChatRequest
        {
            Model = _settings.Groq.Model,
            Messages = messages,
            Stream = stream,
            Temperature = _settings.Groq.Options?.Temperature,
            TopP = _settings.Groq.Options?.TopP,
            MaxTokens = maxCompletion
        };
    }

    /// <summary>
    /// Maps a <see cref="GroqChatResponse"/> to <see cref="OllamaChatResponse"/>
    /// so existing consumers receive the expected type.
    /// </summary>
    private static OllamaChatResponse MapToOllamaChatResponse(GroqChatResponse groq)
    {
        var choice = groq.Choices.FirstOrDefault();
        var content = choice?.Message?.Content ?? string.Empty;

        return new OllamaChatResponse
        {
            Model = groq.Model,
            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(groq.Created).UtcDateTime,
            Message = new OllamaMessage { Role = "assistant", Content = content },
            Done = true,
            DoneReason = choice?.FinishReason ?? "stop",
            PromptEvalCount = groq.Usage?.PromptTokens,
            EvalCount = groq.Usage?.CompletionTokens,
            // Duration fields not available from Groq
            TotalDuration = null,
            LoadDuration = null,
            PromptEvalDuration = null,
            EvalDuration = null
        };
    }

    /// <summary>
    /// Strips markdown code fences and deserializes JSON.
    /// </summary>
    private T DeserializeResponse<T>(string jsonResponse, string contentType)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            _logger.LogError("Received empty response when deserializing {ContentType}", contentType);
            throw new InvalidOperationException($"Groq returned empty response for {contentType}");
        }

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
                throw new InvalidOperationException($"Failed to parse {contentType} from Groq response");
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
