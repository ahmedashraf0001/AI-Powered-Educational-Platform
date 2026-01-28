using AIEduPlatform.Core.Domain.Context;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Responses.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Responses.Grading;
using AIEduPlatform.Core.DTOs.AI.Responses.MindMap;
using AIEduPlatform.Core.DTOs.AI.Responses.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Responses.Quiz;
using AIEduPlatform.Core.DTOs.AI.Responses.Summarization;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IOllamaService
    {
        // Core generation methods
        Task<OllamaGenerateResponse> GenerateResponseAsync(OllamaGenerateRequest req);
        Task<OllamaGenerateResponse> GenerateStreamResponseAsync(OllamaGenerateRequest req, Action<string>? onChunk = null);

        // Study Chat
        Task<OllamaChatResponse> GenerateStudyChatResponseAsync(
            List<ContextChunk> contextChunks,
            string userQuestion,           
            List<ChatMessage>? conversationHistory = null,
            CancellationToken? ct = null);

        // Summarization
        Task<SummarizationResponse> GenerateSummaryAsync(
            List<ContextChunk> contextChunks,
            int summaryLength = 500,
            bool includeKeyPoints = true,
            CancellationToken? ct = null);

        // Flashcards
        Task<List<FlashcardResponse>> GenerateFlashcardsAsync(
            List<ContextChunk> contextChunks,
            string topic,
            int numOfCards = 10, CancellationToken? ct = null);

        // Mind Map
        Task<MindMapResponse> GenerateMindMapAsync(
            List<ContextChunk> contextChunks,
            string centralTopic,
            int maxDepth = 3,
            CancellationToken? ct = null);

        // Quiz
        Task<List<QuizResponse>> GenerateQuizAsync(
            List<ContextChunk> contextChunks,
            string topic,
            int numberOfQuestions,
            string difficulty,
            List<string> questionTypes, CancellationToken? ct = null);

        // Essay Grading
        Task<EssayGradingResponse> GradeEssayAsync(
            List<ContextChunk> contextChunks,
            string questionText,
            int maxPoints,
            string modelAnswer,
            string studentAnswer,
            CancellationToken? ct = null);

        // Question Generation
        Task<QuestionGenerationResponse> GenerateExamQuestionsAsync(
            List<ContextChunk> contextChunks,
            int numberOfQuestions,
            string difficulty,
            List<string> questionTypes,
            List<string>? focusTopics = null,
            CancellationToken? ct = null);

        // Model management
        Task<bool> IsModelAvailableAsync(string model);
        Task<List<string>> GetAvailableModelsAsync();
    }

}
