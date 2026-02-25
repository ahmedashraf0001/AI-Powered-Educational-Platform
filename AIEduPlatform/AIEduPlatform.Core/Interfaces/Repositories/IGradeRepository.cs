using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for Grade entity operations.
    /// Manages grades for exam submissions including AI grading and teacher approval.
    /// </summary>
    public interface IGradeRepository : IGenericRepository<Grade>
    {
        /// <summary>
        /// Gets a grade by submission ID
        /// </summary>
        Task<Grade?> GetGradeBySubmissionIdAsync(
            Guid submissionId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all grades for a student
        /// </summary>
        Task<List<Grade>> GetGradesByStudentIdAsync(
            Guid studentId,
            bool includeSubmission = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all grades for an exam
        /// </summary>
        Task<List<Grade>> GetGradesByExamIdAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all grades for a student in a specific course
        /// </summary>
        Task<List<Grade>> GetGradesByStudentAndCourseAsync(
            Guid studentId,
            Guid courseId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets grades that were AI-graded but not yet approved
        /// </summary>
        Task<List<Grade>> GetPendingApprovalGradesAsync(
            Guid? examId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Approves an AI-graded submission
        /// </summary>
        Task ApproveGradeAsync(
            Guid gradeId,
            CancellationToken ct = default);

        /// <summary>
        /// Updates a grade with teacher corrections
        /// </summary>
        Task UpdateGradeAsync(
            Guid gradeId,
            float newScore,
            string feedback,
            CancellationToken ct = default);

        /// <summary>
        /// Gets grade statistics for a student
        /// </summary>
        Task<StudentGradeStats> GetStudentStatsAsync(
            Guid studentId,
            Guid? courseId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Gets grade statistics for an exam
        /// </summary>
        Task<ExamGradeStats> GetExamStatsAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets grade distribution for an exam (for analytics)
        /// </summary>
        Task<Dictionary<string, int>> GetGradeDistributionAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a grade with its submission, exam, and course info for authorization checks.
        /// Reduces N+1 queries by fetching all needed data in a single query.
        /// </summary>
        Task<Grade?> GetGradeWithSubmissionExamAndCourseAsync(
            Guid gradeId,
            CancellationToken ct = default);
    }
}
