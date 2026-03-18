using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for ExamAttempt entity operations.
    /// Manages student exam attempt tracking for timer persistence.
    /// </summary>
    public interface IExamAttemptRepository : IGenericRepository<ExamAttempt>
    {
        /// <summary>
        /// Gets an attempt by exam ID and student ID
        /// </summary>
        Task<ExamAttempt?> GetByExamAndStudentAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets or creates an attempt for a student on an exam
        /// </summary>
        Task<ExamAttempt> GetOrCreateAttemptAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Marks an attempt as submitted
        /// </summary>
        Task MarkAsSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Saves the current answers for an attempt
        /// </summary>
        Task SaveAnswersAsync(
            Guid examId,
            Guid studentId,
            string answersJson,
            CancellationToken ct = default);
    }
}
