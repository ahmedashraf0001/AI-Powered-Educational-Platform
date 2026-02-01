using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using Flashcard = AIEduPlatform.Core.DTOs.AI.Simple.Flashcard;
namespace AIEduPlatform.Core.Interfaces.Services;

/// <summary>
/// Ollama AI service client for generation, study features, and model management.
/// Uses simple DTOs that directly match AI prompt response formats.
/// </summary>
public interface IOllamaServiceClient
{
    #region Core Generation Methods

    /// <summary>
    /// Sends a raw generation request to Ollama.
    /// </summary>
    Task<OllamaGenerateResponse> GenerateAsync(
        string prompt,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a raw generation request and streams the response.
    /// </summary>
    IAsyncEnumerable<OllamaGenerateStreamChunk> GenerateStreamAsync(
        string prompt,
        CancellationToken ct = default);

    #endregion

    #region Study Studio Features

    /// <summary>
    /// Generates a chat response grounded in the provided context and conversation history.
    /// </summary>
    Task<ChatResponse> GenerateStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<OllamaMessage>? conversationHistory = null,
        CancellationToken ct = default);

    /// <summary>
    /// Streams a chat response grounded in the provided context and conversation history.
    /// </summary>
    IAsyncEnumerable<OllamaGenerateStreamChunk> GenerateStreamStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<OllamaMessage>? conversationHistory = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generates flashcards from the provided context chunks.
    /// </summary>
    Task<List<Flashcard>> GenerateFlashcardsAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfCards = 10,
        CancellationToken ct = default);


    /// <summary>
    /// Generates a mind map from the provided context chunks.
    /// </summary>
    Task<MindMapNode> GenerateMindMapAsync(
        List<ContextChunk> contextChunks,
        string centralTopic,
        int maxDepth = 3,
        CancellationToken ct = default);

    /// <summary>
    /// Generates practice quiz questions from the provided context chunks.
    /// </summary>
    Task<List<QuizQuestion>> GenerateQuizAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfQuestions,
        string difficulty,
        List<QuestionType> questionTypes,
        CancellationToken ct = default);

    #endregion

    #region Content Processing

    /// <summary>
    /// Generates a summary of the provided context chunks.
    /// </summary>
    Task<Summary> GenerateSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default);

    /// <summary>
    /// Streams a summary of the provided context chunks.
    /// </summary>
    IAsyncEnumerable<Summary> GenerateStreamSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default);

    #endregion

    #region Teacher Features

    /// <summary>
    /// Grades a student's essay answer against a model answer using AI.
    /// </summary>
    Task<EssayGrade> GradeEssayAsync(
        List<ContextChunk> contextChunks,
        string questionText,
        int maxPoints,
        string modelAnswer,
        string studentAnswer,
        CancellationToken ct = default);

    /// <summary>
    /// Generates exam questions from the provided context chunks.
    /// </summary>
    Task<List<ExamQuestion>> GenerateExamQuestionsAsync(
        List<ContextChunk> contextChunks,
        int numberOfQuestions,
        string difficulty,
        List<string> questionTypes,
        List<string>? focusTopics = null,
        CancellationToken ct = default);

    #endregion

    #region Model Management

    /// <summary>
    /// Checks whether a specific model is available on the Ollama server.
    /// </summary>
    Task<bool> IsModelAvailableAsync(string model, CancellationToken ct = default);

    /// <summary>
    /// Returns all models available on the Ollama server.
    /// </summary>
    Task<List<string>> GetAvailableModelsAsync(CancellationToken ct = default);

    #endregion
}