using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for Question entity operations.
    /// Manages exam questions including creation, retrieval, and bulk operations.
    /// </summary>
    public interface IQuestionRepository : IGenericRepository<Question>
    {
        /// <summary>
        /// Gets all questions for an exam
        /// </summary>
        Task<List<Question>> GetQuestionsByExamIdAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets questions for an exam filtered by type
        /// </summary>
        Task<List<Question>> GetQuestionsByTypeAsync(
            Guid examId,
            QuestionType type,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the total points for all questions in an exam
        /// </summary>
        Task<int> GetTotalPointsForExamAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Adds multiple questions to an exam at once
        /// </summary>
        Task AddQuestionsToExamAsync(
            Guid examId,
            List<Question> questions,
            CancellationToken ct = default);

        /// <summary>
        /// Deletes all questions for an exam
        /// </summary>
        Task<int> DeleteQuestionsByExamIdAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Reorders questions within an exam using explicit order values
        /// </summary>
        Task ReorderQuestionsAsync(
            Guid examId,
            Dictionary<Guid, int> questionOrders,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the maximum Order value for questions in an exam
        /// </summary>
        Task<int> GetMaxOrderForExamAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets question count by type for an exam
        /// </summary>
        Task<Dictionary<QuestionType, int>> GetQuestionCountByTypeAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a question with its exam and course for authorization checks.
        /// Reduces N+1 queries by fetching all needed data in a single query.
        /// </summary>
        Task<Question?> GetQuestionWithExamAndCourseAsync(
            Guid questionId,
            CancellationToken ct = default);
    }
}
