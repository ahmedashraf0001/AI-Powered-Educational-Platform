using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;

namespace AIEduPlatform.Core.Interfaces.Repositories
{
    /// <summary>
    /// Repository for Submission entity operations.
    /// Manages student exam submissions and their answers.
    /// </summary>
    public interface ISubmissionRepository : IGenericRepository<Submission>
    {
        /// <summary>
        /// Gets a submission by ID with optional related data
        /// </summary>
        Task<Submission?> GetSubmissionByIdAsync(
            Guid submissionId,
            bool includeExam = false,
            bool includeGrade = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a student's submission for a specific exam
        /// </summary>
        Task<Submission?> GetSubmissionByExamAndStudentAsync(
            Guid examId,
            Guid studentId,
            bool includeGrade = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all submissions for an exam
        /// </summary>
        Task<List<Submission>> GetSubmissionsByExamIdAsync(
            Guid examId,
            bool includeGrades = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all submissions by a student
        /// </summary>
        Task<List<Submission>> GetSubmissionsByStudentIdAsync(
            Guid studentId,
            bool includeExam = false,
            bool includeGrade = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets all submissions by a student for exams in a specific course
        /// </summary>
        Task<List<Submission>> GetSubmissionsByStudentAndCourseAsync(
            Guid studentId,
            Guid courseId,
            bool includeGrade = false,
            CancellationToken ct = default);

        /// <summary>
        /// Gets submissions that haven't been graded yet
        /// </summary>
        Task<List<Submission>> GetUngradedSubmissionsAsync(
            Guid? examId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Gets submissions that were AI-graded but not yet approved by a teacher
        /// </summary>
        Task<List<Submission>> GetPendingApprovalSubmissionsAsync(
            Guid? examId = null,
            CancellationToken ct = default);

        /// <summary>
        /// Gets submission count for an exam
        /// </summary>
        Task<int> GetSubmissionCountAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Checks if a student has submitted an exam
        /// </summary>
        Task<bool> HasStudentSubmittedAsync(
            Guid examId,
            Guid studentId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets submission statistics for an exam
        /// </summary>
        Task<SubmissionStats> GetExamStatsAsync(
            Guid examId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets a submission with exam and course info for authorization checks.
        /// Reduces N+1 queries by fetching all needed data in a single query.
        /// </summary>
        Task<Submission?> GetSubmissionWithExamAndCourseAsync(
            Guid submissionId,
            CancellationToken ct = default);

        /// <summary>
        /// Gets submissions that have a placeholder grade (from auto-grading) but haven't been AI-graded yet.
        /// These are submissions with Grade != null and !Grade.IsAiGraded.
        /// </summary>
        Task<List<Submission>> GetPendingAIGradingSubmissionsAsync(
            CancellationToken ct = default);
    }      
}
