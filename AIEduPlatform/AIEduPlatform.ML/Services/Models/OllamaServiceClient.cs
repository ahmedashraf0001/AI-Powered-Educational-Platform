using AIEduPlatform.Core.Domain.Context;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Ollama;
using AIEduPlatform.Core.DTOs.AI.Responses.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Responses.Grading;
using AIEduPlatform.Core.DTOs.AI.Responses.MindMap;
using AIEduPlatform.Core.DTOs.AI.Responses.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Responses.Quiz;
using AIEduPlatform.Core.DTOs.AI.Responses.Summarization;
using AIEduPlatform.Core.Interfaces.Services;

namespace AIEduPlatform.ML.Services.Models
{
    public class OllamaServiceClient : IOllamaService
    {
        private readonly IRAGService _RAGService;

        public OllamaServiceClient()
        {
            
        }

        public Task<QuestionGenerationResponse> GenerateExamQuestionsAsync(List<ContextChunk> contextChunks, int numberOfQuestions, string difficulty, List<string> questionTypes, List<string>? focusTopics = null, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<FlashcardResponse>> GenerateFlashcardsAsync(List<ContextChunk> contextChunks, string topic, int numOfCards = 10, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<MindMapResponse> GenerateMindMapAsync(List<ContextChunk> contextChunks, string centralTopic, int maxDepth = 3, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<QuizResponse>> GenerateQuizAsync(List<ContextChunk> contextChunks, string topic, int numberOfQuestions, string difficulty, List<string> questionTypes, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<OllamaGenerateResponse> GenerateResponseAsync(OllamaGenerateRequest req)
        {
            throw new NotImplementedException();
        }

        public Task<OllamaGenerateResponse> GenerateStreamResponseAsync(OllamaGenerateRequest req, Action<string>? onChunk = null)
        {
            throw new NotImplementedException();
        }

        public Task<OllamaChatResponse> GenerateStudyChatResponseAsync(List<ContextChunk> contextChunks, string userQuestion, List<ChatMessage>? conversationHistory = null, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<SummarizationResponse> GenerateSummaryAsync(List<ContextChunk> contextChunks, int summaryLength = 500, bool includeKeyPoints = true, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<string>> GetAvailableModelsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<EssayGradingResponse> GradeEssayAsync(List<ContextChunk> contextChunks, string questionText, int maxPoints, string modelAnswer, string studentAnswer, CancellationToken? ct = null)
        {
            throw new NotImplementedException();
        }

        public Task<bool> IsModelAvailableAsync(string model)
        {
            throw new NotImplementedException();
        }
    }
}
