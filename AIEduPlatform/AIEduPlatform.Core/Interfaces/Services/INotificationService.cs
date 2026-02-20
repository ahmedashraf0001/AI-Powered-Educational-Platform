using AIEduPlatform.Core.DTOs.RAG;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIEduPlatform.Core.Interfaces.Services
{
    public interface INotificationService
    {
        // ─── Teacher-targeted Notifications (MaterialIndexingHub) ───

        /// <summary>
        /// Notify a specific teacher about indexing completion
        /// </summary>
        Task NotifyIndexingCompletedAsync(Guid userId, RagIndexResponse response, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify the teacher that a student submitted an exam
        /// </summary>
        Task NotifyExamSubmittedAsync(Guid teacherId, string studentName, string examTitle, string courseName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify the teacher that a new student enrolled in their course
        /// </summary>
        Task NotifyNewEnrollmentAsync(Guid teacherId, string studentName, string courseName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify the teacher that a student left a review on their course
        /// </summary>
        Task NotifyNewReviewAsync(Guid teacherId, string courseName, int rating, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify the teacher that a student completed their course
        /// </summary>
        Task NotifyEnrollmentCompletedAsync(Guid teacherId, string studentName, string courseName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify the teacher that a student unenrolled from their course
        /// </summary>
        Task NotifyStudentUnenrolledAsync(Guid teacherId, string studentName, string courseName, CancellationToken cancellationToken = default);

        // ─── Student-targeted Notifications (StudentNotificationHub, course group) ───

        /// <summary>
        /// Notify enrolled students that a new exam has been posted
        /// </summary>
        Task NotifyNewExamPostedAsync(Guid courseId, string courseName, string examTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that new course material has been uploaded
        /// </summary>
        Task NotifyNewMaterialUploadedAsync(Guid courseId, string courseName, string materialTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that a new lecture was added
        /// </summary>
        Task NotifyNewLectureAddedAsync(Guid courseId, string courseName, string lectureTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that course details were updated
        /// </summary>
        Task NotifyCourseUpdatedAsync(Guid courseId, string courseName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that the course was published or unpublished
        /// </summary>
        Task NotifyCoursePublishedAsync(Guid courseId, string courseName, bool isPublished, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that an exam's details were changed (schedule, title, etc.)
        /// </summary>
        Task NotifyExamUpdatedAsync(Guid courseId, string courseName, string examTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify enrolled students that an exam was cancelled/deleted
        /// </summary>
        Task NotifyExamDeletedAsync(Guid courseId, string courseName, string examTitle, CancellationToken cancellationToken = default);

        // ─── Student-targeted Notifications (StudentNotificationHub, user-level) ───

        /// <summary>
        /// Notify a student that their submission has been graded
        /// </summary>
        Task NotifySubmissionGradedAsync(Guid studentId, string courseName, string examTitle, decimal score, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify a student that their grade has been approved
        /// </summary>
        Task NotifyGradeApprovedAsync(Guid studentId, string courseName, string examTitle, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify a student that their grade was revised by the teacher
        /// </summary>
        Task NotifyGradeUpdatedAsync(Guid studentId, string courseName, string examTitle, decimal newScore, CancellationToken cancellationToken = default);

        /// <summary>
        /// Notify a student that their engagement is low and the teacher is concerned
        /// </summary>
        Task NotifyLowEngagementAlertAsync(Guid studentId, string courseName, string teacherName, string engagementLevel, string? customMessage = null, CancellationToken cancellationToken = default);
    }
}
