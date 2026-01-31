using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.AI.Requests.Chat;
using AIEduPlatform.Core.DTOs.AI.Requests.Flashcard;
using AIEduPlatform.Core.DTOs.AI.Requests.Grading;
using AIEduPlatform.Core.DTOs.AI.Requests.MindMap;
using AIEduPlatform.Core.DTOs.AI.Requests.QuestionGeneration;
using AIEduPlatform.Core.DTOs.AI.Requests.Quiz;
using AIEduPlatform.Core.DTOs.AI.Requests.Summarization;

namespace AIEduPlatform.Core.Interfaces.Services;

/// <summary>
/// Service interface for Study Session operations.
/// Uses high-level request DTOs (with session context) and simple response DTOs (direct AI output).
/// </summary>
public interface IStudySessionService
{
    /// <summary>
    /// Handles a chat message in a study session
    /// </summary>
    Task<ChatResponse> ChatAsync(StudyChatRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates flashcards for a study session
    /// </summary>
    Task<List<Flashcard>> BuildFlashCardsAsync(FlashcardRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates a practice quiz for a study session
    /// </summary>
    Task<List<QuizQuestion>> GenerateQuizAsync(QuizRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates a mind map for a study session
    /// </summary>
    Task<MindMapNode> BuildMindMapAsync(MindMapRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates a summary for a study session
    /// </summary>
    Task<Summary> GenerateSummaryAsync(SummarizationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Grades an essay answer in a study session
    /// </summary>
    Task<EssayGrade> GradeEssayAsync(EssayGradingRequest request, CancellationToken ct = default);

    /// <summary>
    /// Generates exam questions for a study session
    /// </summary>
    Task<List<ExamQuestion>> GenerateExamQuestionsAsync(QuestionGenerationRequest request, CancellationToken ct = default);
}
