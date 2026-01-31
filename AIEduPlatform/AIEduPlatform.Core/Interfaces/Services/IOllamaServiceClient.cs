using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;

namespace AIEduPlatform.Core.Interfaces.Services;

/// <summary>
/// Interface for Ollama AI service client operations
/// Uses simple DTOs that directly match AI prompt response formats
/// </summary>
public interface IOllamaServiceClient
{
    #region Core Generation Methods

    /// <summary>
    /// Sends a raw generation request to Ollama
    /// </summary>
    Task<OllamaGenerateResponse> GenerateResponseAsync(
        OllamaGenerateRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Sends a raw generation request with streaming response
    /// </summary>
    Task<OllamaGenerateResponse> GenerateStreamResponseAsync(
        OllamaGenerateRequest request,
        Action<string>? onChunk = null,
        CancellationToken ct = default);

    #endregion

    #region Study Studio Features

    /// <summary>
    /// Generates a study chat response based on context and conversation history
    /// </summary>
    Task<ChatResponse> GenerateStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<ChatMessage>? conversationHistory = null,
        CancellationToken ct = default);

    /// <summary>
    /// Generates flashcards from course materials
    /// </summary>
    Task<List<Flashcard>> GenerateFlashcardsAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfCards = 10,
        CancellationToken ct = default);

    /// <summary>
    /// Generates a mind map from course materials
    /// </summary>
    Task<MindMapNode> GenerateMindMapAsync(
        List<ContextChunk> contextChunks,
        string centralTopic,
        int maxDepth = 3,
        CancellationToken ct = default);

    /// <summary>
    /// Generates practice quiz questions from course materials
    /// </summary>
    Task<List<QuizQuestion>> GenerateQuizAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfQuestions,
        string difficulty,
        List<string> questionTypes,
        CancellationToken ct = default);

    #endregion

    #region Content Processing

    /// <summary>
    /// Generates a summary of the provided content
    /// </summary>
    Task<Summary> GenerateSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default);

    #endregion

    #region Teacher Features

    /// <summary>
    /// Grades a student essay answer using AI
    /// </summary>
    Task<EssayGrade> GradeEssayAsync(
        List<ContextChunk> contextChunks,
        string questionText,
        int maxPoints,
        string modelAnswer,
        string studentAnswer,
        CancellationToken ct = default);

    /// <summary>
    /// Generates exam questions for teachers
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
    /// Checks if a specific model is available on the Ollama server
    /// </summary>
    Task<bool> IsModelAvailableAsync(string model, CancellationToken ct = default);

    /// <summary>
    /// Gets a list of all available models on the Ollama server
    /// </summary>
    Task<List<string>> GetAvailableModelsAsync(CancellationToken ct = default);

    #endregion
}
