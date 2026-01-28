using AIEduPlatform.Core.DTOs.AI.Requests.Chat;
using AIEduPlatform.Core.DTOs.AI.Requests.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Requests.Grading;
using AIEduPlatform.Core.DTOs.AI.Requests.MindMap;
using AIEduPlatform.Core.DTOs.AI.Requests.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Requests.Quiz;
using AIEduPlatform.Core.DTOs.AI.Requests.Summarization;
using AIEduPlatform.Core.DTOs.AI.Responses.Chat;
using AIEduPlatform.Core.DTOs.AI.Responses.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Responses.Grading;
using AIEduPlatform.Core.DTOs.AI.Responses.MindMap;
using AIEduPlatform.Core.DTOs.AI.Responses.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Responses.Quiz;
using AIEduPlatform.Core.DTOs.AI.Responses.Summarization;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface IStudySessionService
    {
        Task<StudyChatResponse> ChatAsync(StudyChatRequest req, CancellationToken ct);
        Task<FlashcardResponse> BuildFlashCardAsync(FlashcardRequest req, CancellationToken ct);
        Task<QuizResponse> GenerateQuizAsync(QuizRequest req, CancellationToken ct);
        Task<MindMapResponse> BuildMindMapAsync(MindMapRequest req, CancellationToken ct);
        Task<SummarizationResponse> GenerateSummaryAsync(SummarizationRequest req, CancellationToken ct);
        Task<EssayGradingResponse> GradeEssayAsync(EssayGradingRequest req, CancellationToken ct);
        Task<QuestionGenerationResponse> GenerateExamQuestionsAsync(QuestionGenerationRequest req, CancellationToken ct);
    }
}
