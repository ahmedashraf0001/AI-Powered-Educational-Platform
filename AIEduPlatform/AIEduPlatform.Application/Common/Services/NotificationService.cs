using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace AIEduPlatform.Application.Common.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<MaterialIndexingHub> _teacherHubContext;
        private readonly IHubContext<StudentNotificationHub> _studentHubContext;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<MaterialIndexingHub> teacherHubContext,
            IHubContext<StudentNotificationHub> studentHubContext,
            ILogger<NotificationService> logger)
        {
            _teacherHubContext = teacherHubContext;
            _studentHubContext = studentHubContext;
            _logger = logger;
        }

        public async Task NotifyIndexingCompletedAsync(
            Guid userId,
            RagIndexResponse response,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Sending indexing notification to teacher. UserId: {UserId}, CourseId: {CourseId}, Success: {Success}",
                    userId, response.CourseId, response.Success);

                await _teacherHubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveIndexingNotification", response, cancellationToken);

                _logger.LogInformation(
                    "Indexing notification sent successfully. UserId: {UserId}, CourseId: {CourseId}",
                    userId, response.CourseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send indexing notification. UserId: {UserId}, CourseId: {CourseId}",
                    userId, response.CourseId);
            }
        }

        public async Task NotifyNewExamPostedAsync(
            Guid courseId,
            string courseName,
            string examTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about new exam. CourseId: {CourseId}, ExamTitle: {ExamTitle}",
                    courseId, examTitle);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("NewExamPosted", new { CourseId = courseId, CourseName = courseName, ExamTitle = examTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send new exam notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        public async Task NotifySubmissionGradedAsync(
            Guid studentId,
            string courseName,
            string examTitle,
            decimal score,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying student about graded submission. StudentId: {StudentId}, ExamTitle: {ExamTitle}",
                    studentId, examTitle);

                await _studentHubContext.Clients
                    .User(studentId.ToString())
                    .SendAsync("SubmissionGraded", new { CourseName = courseName, ExamTitle = examTitle, Score = score }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send graded notification. StudentId: {StudentId}",
                    studentId);
            }
        }

        public async Task NotifyGradeApprovedAsync(
            Guid studentId,
            string courseName,
            string examTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying student about approved grade. StudentId: {StudentId}, ExamTitle: {ExamTitle}",
                    studentId, examTitle);

                await _studentHubContext.Clients
                    .User(studentId.ToString())
                    .SendAsync("GradeApproved", new { CourseName = courseName, ExamTitle = examTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send grade approved notification. StudentId: {StudentId}",
                    studentId);
            }
        }

        public async Task NotifyNewMaterialUploadedAsync(
            Guid courseId,
            string courseName,
            string materialTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about new material. CourseId: {CourseId}, MaterialTitle: {MaterialTitle}",
                    courseId, materialTitle);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("NewMaterialUploaded", new { CourseId = courseId, CourseName = courseName, MaterialTitle = materialTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send new material notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        // ─── Teacher-targeted notifications ───

        public async Task NotifyExamSubmittedAsync(
            Guid teacherId,
            string studentName,
            string examTitle,
            string courseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about exam submission. TeacherId: {TeacherId}, Student: {StudentName}, Exam: {ExamTitle}",
                    teacherId, studentName, examTitle);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("ExamSubmitted", new { StudentName = studentName, ExamTitle = examTitle, CourseName = courseName }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send exam submitted notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }

        public async Task NotifyNewEnrollmentAsync(
            Guid teacherId,
            string studentName,
            string courseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about new enrollment. TeacherId: {TeacherId}, Student: {StudentName}, Course: {CourseName}",
                    teacherId, studentName, courseName);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("NewEnrollment", new { StudentName = studentName, CourseName = courseName }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send enrollment notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }

        public async Task NotifyNewReviewAsync(
            Guid teacherId,
            string courseName,
            int rating,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about new review. TeacherId: {TeacherId}, Course: {CourseName}, Rating: {Rating}",
                    teacherId, courseName, rating);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("NewReview", new { CourseName = courseName, Rating = rating }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send review notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }

        public async Task NotifyEnrollmentCompletedAsync(
            Guid teacherId,
            string studentName,
            string courseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about course completion. TeacherId: {TeacherId}, Student: {StudentName}, Course: {CourseName}",
                    teacherId, studentName, courseName);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("EnrollmentCompleted", new { StudentName = studentName, CourseName = courseName }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send completion notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }

        public async Task NotifyStudentUnenrolledAsync(
            Guid teacherId,
            string studentName,
            string courseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about student unenrollment. TeacherId: {TeacherId}, Student: {StudentName}, Course: {CourseName}",
                    teacherId, studentName, courseName);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("StudentUnenrolled", new { StudentName = studentName, CourseName = courseName }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send unenrollment notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }

        // ─── Student-targeted notifications (course group) ───

        public async Task NotifyNewLectureAddedAsync(
            Guid courseId,
            string courseName,
            string lectureTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about new lecture. CourseId: {CourseId}, Lecture: {LectureTitle}",
                    courseId, lectureTitle);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("NewLectureAdded", new { CourseId = courseId, CourseName = courseName, LectureTitle = lectureTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send new lecture notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        public async Task NotifyCourseUpdatedAsync(
            Guid courseId,
            string courseName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about course update. CourseId: {CourseId}, Course: {CourseName}",
                    courseId, courseName);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("CourseUpdated", new { CourseId = courseId, CourseName = courseName }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send course updated notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        public async Task NotifyCoursePublishedAsync(
            Guid courseId,
            string courseName,
            bool isPublished,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about course publish state change. CourseId: {CourseId}, IsPublished: {IsPublished}",
                    courseId, isPublished);

                var eventName = isPublished ? "CoursePublished" : "CourseUnpublished";
                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync(eventName, new { CourseId = courseId, CourseName = courseName, IsPublished = isPublished }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send course published notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        public async Task NotifyExamUpdatedAsync(
            Guid courseId,
            string courseName,
            string examTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about exam update. CourseId: {CourseId}, Exam: {ExamTitle}",
                    courseId, examTitle);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("ExamUpdated", new { CourseId = courseId, CourseName = courseName, ExamTitle = examTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send exam updated notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        public async Task NotifyExamDeletedAsync(
            Guid courseId,
            string courseName,
            string examTitle,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying students about exam deletion. CourseId: {CourseId}, Exam: {ExamTitle}",
                    courseId, examTitle);

                await _studentHubContext.Clients
                    .Group($"course-{courseId}")
                    .SendAsync("ExamDeleted", new { CourseId = courseId, CourseName = courseName, ExamTitle = examTitle }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send exam deleted notification. CourseId: {CourseId}",
                    courseId);
            }
        }

        // ─── Student-targeted notifications (user-level) ───

        public async Task NotifyGradeUpdatedAsync(
            Guid studentId,
            string courseName,
            string examTitle,
            decimal newScore,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying student about grade update. StudentId: {StudentId}, Exam: {ExamTitle}, NewScore: {NewScore}",
                    studentId, examTitle, newScore);

                await _studentHubContext.Clients
                    .User(studentId.ToString())
                    .SendAsync("GradeUpdated", new { CourseName = courseName, ExamTitle = examTitle, NewScore = newScore }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send grade updated notification. StudentId: {StudentId}",
                    studentId);
            }
        }

        public async Task NotifyLowEngagementAlertAsync(
            Guid studentId,
            string courseName,
            string teacherName,
            string engagementLevel,
            string? customMessage = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Sending engagement alert. StudentId: {StudentId}, Course: {CourseName}, Level: {Level}",
                    studentId, courseName, engagementLevel);

                await _studentHubContext.Clients
                    .User(studentId.ToString())
                    .SendAsync("EngagementAlert", new
                    {
                        CourseName = courseName,
                        TeacherName = teacherName,
                        EngagementLevel = engagementLevel,
                        Message = customMessage
                            ?? $"Your teacher has noticed low engagement in {courseName}. Consider reviewing the course materials and completing any pending assignments.",
                        SentAt = DateTime.UtcNow
                    }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send engagement alert. StudentId: {StudentId}",
                    studentId);
            }
        }
    }
}