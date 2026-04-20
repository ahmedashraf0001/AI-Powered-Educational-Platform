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

        topic = string.IsNullOrWhiteSpace(topic) ? "the main concepts from the selected materials" : topic;

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

        centralTopic = string.IsNullOrWhiteSpace(centralTopic) ? "the main concepts from the selected materials" : centralTopic;

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

        topic = string.IsNullOrWhiteSpace(topic) ? "the main concepts from the selected materials" : topic;

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

    #region Semantic Section Extraction

    public async Task<SemanticSectionExtractionResult> ExtractSemanticSectionsAsync(
        string content,
        bool isTimeBased,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be null or empty.", nameof(content));

        _logger.LogInformation("Extracting semantic sections (isTimeBased={IsTimeBased}), content length={Length}",
            isTimeBased, content.Length);

        var prompt = PromptBuilder.BuildSemanticSectionMessages(content, isTimeBased);
        var chatResponse = await ChatAsync(prompt, ct);

        return DeserializeResponse<SemanticSectionExtractionResult>(
            chatResponse.Message.Content, "semantic sections");
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

    // ── Token-budget constants for Ollama (local, generous) ──────────────
    // Ollama context windows are finite (NumCtx, default 8192).
    // We budget loosely: keep as much history as fits, only trim when needed.
    private const int DefaultOllamaContextTokens = 16384;
    private const int OllamaTokenSafetyMargin    = 150;     // JSON/role tag overhead
    private const int DefaultOllamaMaxCompletion  = 4096;
    private const int OllamaMaxHistoryMessages    = 20;     // generous — local model, no rate limit

    /// <summary>
    /// Rough token estimate (~3.5 chars per token for English).
    /// </summary>
    private static int EstimateTokens(string? text)
        => string.IsNullOrEmpty(text) ? 0 : (int)Math.Ceiling(text.Length / 3.5);

    /// <summary>
    /// Builds an OllamaChatRequest with system + user messages for /api/chat.
    /// Priority order: 1) Instructions (system) — fully preserved
    ///                 2) Content/RAG (user message) — trimmed only as last resort
    ///                 3) Chat history — first to be dropped when budget is tight
    /// Less aggressive than the Groq variant — local model, bigger context.
    /// </summary>
    private OllamaChatRequest BuildChatRequest(PromptResult prompt, bool stream)
    {
        if (_settings.Ollama == null)
            throw new InvalidOperationException("Ollama settings are not configured");

        var contextWindow = _settings.Ollama.Options?.NumCtx ?? DefaultOllamaContextTokens;
        var maxCompletion = _settings.Ollama.Options?.NumPredict ?? DefaultOllamaMaxCompletion;

        // Total budget for all input messages
        var inputBudget = contextWindow - maxCompletion - OllamaTokenSafetyMargin;
        if (inputBudget < 500)
        {
            _logger.LogWarning(
                "Ollama input budget is very small ({Budget} tokens). NumCtx={NumCtx}, NumPredict={NumPredict}",
                inputBudget, contextWindow, maxCompletion);
            inputBudget = 500;
        }

        // ── PRIORITY 1: Instructions (system prompt) — takes what it needs first ──
        // Only truncated if it alone exceeds 70% of the entire input budget (very generous).
        var systemContent = prompt.SystemMessage;
        var systemTokens  = EstimateTokens(systemContent);
        var systemCeiling  = (int)(inputBudget * 0.50);  // looser — more room for content & history
        if (systemTokens > systemCeiling)
        {
            var maxChars = (int)(systemCeiling * 3.5);
            systemContent = systemContent[..Math.Min(maxChars, systemContent.Length)];
            systemTokens  = EstimateTokens(systemContent);
            _logger.LogWarning("Ollama system prompt truncated from ~{Original} to ~{Truncated} estimated tokens",
                EstimateTokens(prompt.SystemMessage), systemTokens);
        }

        var budgetAfterSystem = inputBudget - systemTokens;

        // ── PRIORITY 2: Content / RAG context (user message) — uses remaining budget freely ──
        // Only truncated if it exceeds what's left after instructions.
        // A soft reserve is kept so at least a few history messages can fit.
        var userContent = prompt.UserMessage;
        var userTokens  = EstimateTokens(userContent);
        var historyReserve = Math.Min((int)(budgetAfterSystem * 0.25), 2000); // generous history reserve
        var userCeiling = budgetAfterSystem - historyReserve;
        if (userCeiling < 300) userCeiling = budgetAfterSystem; // if budget is tiny, skip history reserve

        if (userTokens > userCeiling)
        {
            var maxChars = (int)(userCeiling * 3.5);
            userContent = userContent[..Math.Min(maxChars, userContent.Length)]
                          + "\n\n[Content truncated to fit context window]";
            userTokens  = EstimateTokens(userContent);
            _logger.LogWarning("Ollama user/RAG content truncated from ~{Original} to ~{Truncated} estimated tokens",
                EstimateTokens(prompt.UserMessage), userTokens);
        }

        // ── PRIORITY 3: Chat history — gets whatever is left ──
        var remainingBudget = inputBudget - systemTokens - userTokens;

        var messages = new List<OllamaMessage>
        {
            new OllamaMessage { Role = "system", Content = systemContent }
        };

        if (prompt.ConversationHistory != null && prompt.ConversationHistory.Any() && remainingBudget > 80)
        {
            var recentHistory = prompt.ConversationHistory
                .Where(m => m.Role == "user" || m.Role == "assistant")
                .TakeLast(OllamaMaxHistoryMessages)
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
                    messages.Add(new OllamaMessage
                    {
                        Role = historyMsg.Role,
                        Content = historyMsg.Content
                    });
                }

                _logger.LogDebug("Included {Count} history messages (~{Tokens} tokens) in Ollama request",
                    recentHistory.Count,
                    recentHistory.Sum(m => EstimateTokens(m.Content)));
            }
            else
            {
                _logger.LogWarning("All conversation history dropped to fit Ollama context window ({Budget} tokens available)",
                    remainingBudget);
            }
        }

        // Final user message always goes last
        messages.Add(new OllamaMessage { Role = "user", Content = userContent });

        var totalEstimated = messages.Sum(m => EstimateTokens(m.Content) + 4) + maxCompletion;
        _logger.LogDebug(
            "Ollama budget: system ~{SysTokens} + content ~{UserTokens} + history ~{HistTokens} + completion {MaxComp} = ~{Total} (ctx {CtxSize})",
            systemTokens, userTokens,
            messages.Where(m => m.Role != "system").Sum(m => EstimateTokens(m.Content)) - userTokens,
            maxCompletion, totalEstimated, contextWindow);

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
    /// then deserializes the JSON. Attempts to repair truncated JSON arrays when the
    /// LLM output exceeded the token limit.
    /// </summary>
    private T DeserializeResponse<T>(string jsonResponse, string contentType)
    {
        if (string.IsNullOrWhiteSpace(jsonResponse))
        {
            _logger.LogError("Received empty response when deserializing {ContentType}", contentType);
            throw new InvalidOperationException($"Ollama returned empty response for {contentType}");
        }

        var cleaned = StripMarkdownFence(jsonResponse, contentType);

        try
        {
            var options = new JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            };
            var result = JsonSerializer.Deserialize<T>(cleaned, options);

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
            _logger.LogWarning(ex,
                "Initial JSON deserialization failed for {ContentType}, attempting truncated JSON repair",
                contentType);

            // Attempt to repair truncated JSON (e.g. LLM hit token limit mid-output)
            var repaired = TryRepairTruncatedJson(cleaned);
            if (repaired != null)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<T>(repaired);
                    if (result != null)
                    {
                        _logger.LogInformation(
                            "Successfully repaired truncated {ContentType} JSON response",
                            contentType);
                        return result;
                    }
                }
                catch (JsonException)
                {
                    // Repair didn't produce valid JSON either — fall through to original error
                }
            }

            _logger.LogError(ex, "JSON deserialization failed for {ContentType}. Response: {Response}",
                contentType, cleaned);
            throw new InvalidOperationException($"Invalid JSON format for {contentType}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Strips markdown code fences, including truncated fences where the closing ``` is missing.
    /// </summary>
    private string StripMarkdownFence(string jsonResponse, string contentType)
    {
        var cleaned = jsonResponse.Trim();

        // Try matching a complete fence first: ```json ... ```
        var fenceMatch = MarkdownFenceRegex.Match(cleaned);
        if (fenceMatch.Success)
        {
            cleaned = fenceMatch.Groups[1].Value.Trim();
            _logger.LogDebug("Stripped markdown fence from {ContentType} response", contentType);
            return cleaned;
        }

        // Handle truncated fence (opening ``` without closing ```)
        var openFenceMatch = Regex.Match(cleaned, @"^```(?:json)?\s*\n?", RegexOptions.IgnoreCase);
        if (openFenceMatch.Success)
        {
            cleaned = cleaned[openFenceMatch.Length..].Trim();
            _logger.LogDebug("Stripped unclosed markdown fence from truncated {ContentType} response",
                contentType);
        }

        return cleaned;
    }

    /// <summary>
    /// Attempts to repair truncated JSON by finding the last complete object in an array
    /// and closing the array bracket. Returns null if repair is not possible.
    /// </summary>
    private static string? TryRepairTruncatedJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        var trimmed = json.Trim();

        // Only attempt repair for JSON arrays (the common case: list of quiz questions, flashcards, etc.)
        if (!trimmed.StartsWith('['))
            return null;

        // Find the last complete object by looking for the pattern "},\n" or "}\n" before truncation.
        // We search backwards for the last '}' that closes a complete object.
        var lastCompleteObjEnd = -1;
        var braceDepth = 0;
        var bracketDepth = 0;
        var inString = false;
        var escape = false;

        for (var i = 0; i < trimmed.Length; i++)
        {
            var c = trimmed[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (c == '\\' && inString)
            {
                escape = true;
                continue;
            }

            if (c == '"')
            {
                inString = !inString;
                continue;
            }

            if (inString) continue;

            switch (c)
            {
                case '[': bracketDepth++; break;
                case ']': bracketDepth--; break;
                case '{': braceDepth++; break;
                case '}':
                    braceDepth--;
                    // When we close back to top-level array depth (bracketDepth==1, braceDepth==0),
                    // this is the end of a complete top-level object in the array
                    if (braceDepth == 0 && bracketDepth == 1)
                        lastCompleteObjEnd = i;
                    break;
            }
        }

        if (lastCompleteObjEnd <= 0)
            return null;

        // Build repaired JSON: everything up to and including the last complete object, then close the array
        var repaired = trimmed[..(lastCompleteObjEnd + 1)] + "\n]";
        return repaired;
    }

    #endregion
    public async Task<AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto> ExtractCourseTagsAsync(
        AIEduPlatform.Core.DTOs.Tags.CourseTaggingDto course,
        CancellationToken ct = default)
    {
        if (course == null)
            throw new ArgumentNullException(nameof(course));

        if (string.IsNullOrWhiteSpace(course.Title))
            throw new ArgumentException("Course title is required.");

        // Build prompt
        var prompt = PromptBuilder.BuildTagExtractionMessages(course);

        // Call LLM
        var chatResponse = await ChatAsync(prompt, ct);

        // Deserialize response
        return DeserializeResponse<AIEduPlatform.Core.DTOs.Tags.CourseTagsResultDto>(
            chatResponse.Message.Content,
            "tag extraction");
    }
}

