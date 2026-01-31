using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.AI.Requests.Chat;
using AIEduPlatform.Core.DTOs.AI.Requests.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Requests.Grading;
using AIEduPlatform.Core.DTOs.AI.Requests.MindMap;
using AIEduPlatform.Core.DTOs.AI.Requests.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Requests.Quiz;
using AIEduPlatform.Core.DTOs.AI.Requests.Summarization;
using AIEduPlatform.Core.Interfaces.Services;

namespace AIEduPlatform.Application.Services;

/// <summary>
/// Implementation of Study Session Service.
/// Wraps IOllamaServiceClient with session context management.
/// </summary>
internal class StudySessionService : IStudySessionService
{
    private readonly IOllamaServiceClient _ollamaClient;
    private readonly IRAGService _ragService;

    public StudySessionService(IOllamaServiceClient ollamaClient, IRAGService ragService)
    {
        _ollamaClient = ollamaClient;
        _ragService = ragService;
    }

    public Task<ChatResponse> ChatAsync(StudyChatRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<Flashcard>> BuildFlashCardsAsync(FlashcardRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<QuizQuestion>> GenerateQuizAsync(QuizRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<MindMapNode> BuildMindMapAsync(MindMapRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<Summary> GenerateSummaryAsync(SummarizationRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<EssayGrade> GradeEssayAsync(EssayGradingRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<List<ExamQuestion>> GenerateExamQuestionsAsync(QuestionGenerationRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
