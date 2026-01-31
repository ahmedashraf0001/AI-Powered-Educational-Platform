using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Services;

namespace AIEduPlatform.ML.Services.Models;

/// <summary>
/// Implementation of Ollama AI service client
/// Uses simple DTOs that directly match AI prompt response formats
/// </summary>
public class OllamaServiceClient : IOllamaServiceClient
{
    private readonly IRAGService _rag;

    public OllamaServiceClient(IRAGService rag)
    {
        _rag = rag;
    }

    #region Core Generation Methods

    public Task<OllamaGenerateResponse> GenerateResponseAsync(
        OllamaGenerateRequest request,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<OllamaGenerateResponse> GenerateStreamResponseAsync(
        OllamaGenerateRequest request,
        Action<string>? onChunk = null,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Study Studio Features

    public Task<ChatResponse> GenerateStudyChatResponseAsync(
        List<ContextChunk> contextChunks,
        string userQuestion,
        List<ChatMessage>? conversationHistory = null,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Flashcard>> GenerateFlashcardsAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfCards = 10,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<MindMapNode> GenerateMindMapAsync(
        List<ContextChunk> contextChunks,
        string centralTopic,
        int maxDepth = 3,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<QuizQuestion>> GenerateQuizAsync(
        List<ContextChunk> contextChunks,
        string topic,
        int numberOfQuestions,
        string difficulty,
        List<string> questionTypes,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Content Processing

    public Task<Summary> GenerateSummaryAsync(
        List<ContextChunk> contextChunks,
        int summaryLength = 500,
        bool includeKeyPoints = true,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Teacher Features

    public Task<EssayGrade> GradeEssayAsync(
        List<ContextChunk> contextChunks,
        string questionText,
        int maxPoints,
        string modelAnswer,
        string studentAnswer,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<ExamQuestion>> GenerateExamQuestionsAsync(
        List<ContextChunk> contextChunks,
        int numberOfQuestions,
        string difficulty,
        List<string> questionTypes,
        List<string>? focusTopics = null,
        CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region Model Management

    public Task<bool> IsModelAvailableAsync(string model, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<string>> GetAvailableModelsAsync(CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    #endregion
}
