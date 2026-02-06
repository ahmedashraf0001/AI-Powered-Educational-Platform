using AIEduPlatform.Core.Domain.Entities;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for Exam entity operations.
    /// Manages exam creation, scheduling, and question management.
    /// </summary>
    public interface IExamRepository : IGenericRepository<Exam>
    {
        /// <summary>
        /// Gets an exam by ID with optional related data
        /// </summary>
        Task<Exam?> GetExamByIdAsync(
            Guid examId,
            bool includeQuestions = false,
            bool includeSubmissions = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all exams for a course
        /// </summary>
        Task<List<Exam>> GetExamsByCourseIdAsync(
            Guid courseId,
            bool includeQuestions = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets upcoming exams for a course (not yet started)
        /// </summary>
        Task<List<Exam>> GetUpcomingExamsAsync(
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets active exams for a course (currently in progress)
        /// </summary>
        Task<List<Exam>> GetActiveExamsAsync(
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets past exams for a course (already ended)
        /// </summary>
        Task<List<Exam>> GetPastExamsAsync(
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all exams a student can take (enrolled courses, not yet submitted)
        /// </summary>
        Task<List<Exam>> GetAvailableExamsForStudentAsync(
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if an exam is currently active (within start and end time)
        /// </summary>
        Task<bool> IsExamActiveAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets the total points for an exam (sum of all question points)
        /// </summary>
        Task<int> GetTotalPointsAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if a student has already submitted an exam
        /// </summary>
        Task<bool> HasStudentSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets an exam with its associated course in a single query.
        /// Reduces N+1 queries when both exam and course data are needed for authorization.
        /// </summary>
        Task<Exam?> GetExamWithCourseAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if the user is the teacher of the course that contains the exam.
        /// Performs a single optimized database query.
        /// </summary>
        Task<bool> IsUserTeacherOfExamAsync(
            Guid examId,
            Guid userId,
            CancellationToken ct = default);
    }
}
