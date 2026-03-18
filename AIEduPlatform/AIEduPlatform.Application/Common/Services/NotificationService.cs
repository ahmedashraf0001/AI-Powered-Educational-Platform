using AIEduPlatform.Application.SignalR;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
namespace AIEduPlatform.Application.Common.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<MaterialIndexingHub> _teacherHubContext;
        private readonly IHubContext<StudentNotificationHub> _studentHubContext;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IHubContext<MaterialIndexingHub> teacherHubContext,
            IHubContext<StudentNotificationHub> studentHubContext,
            IUnitOfWork unitOfWork,
            ILogger<NotificationService> logger)
        {
            _teacherHubContext = teacherHubContext;
            _studentHubContext = studentHubContext;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        private async Task PersistNotificationAsync(
            Guid userId,
            string type,
            string title,
            string message,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Type = type,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                RelatedEntityId = relatedEntityId,
                RelatedEntityType = relatedEntityType
            };
            await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task PersistGroupNotificationsAsync(
            Guid courseId,
            string type,
            string title,
            string message,
            Guid? relatedEntityId = null,
            string? relatedEntityType = null,
            CancellationToken cancellationToken = default)
        {
            var enrollments = await _unitOfWork.Enrollments.GetActiveEnrollmentsByCourseAsync(courseId, cancellationToken);
            foreach (var enrollment in enrollments)
            {
                var notification = new Notification
                {
                    UserId = enrollment.StudentId,
                    Type = type,
                    Title = title,
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    RelatedEntityId = relatedEntityId ?? courseId,
                    RelatedEntityType = relatedEntityType ?? "Course"
                };
                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
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

                await PersistNotificationAsync(userId, "IndexingCompleted",
                    response.Success ? "Indexing Complete" : "Indexing Failed",
                    response.Success
                        ? $"Material indexing completed for course {response.CourseId}"
                        : $"Material indexing failed for course {response.CourseId}",
                    response.CourseId, "Course", cancellationToken);

                await _teacherHubContext.Clients
                    .User(userId.ToString())
                    .SendAsync("ReceiveIndexingNotification", response, cancellationToken);
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

                await PersistGroupNotificationsAsync(courseId, "NewExamPosted",
                    $"New Exam — {courseName}",
                    $"A new exam \"{examTitle}\" has been posted in {courseName}.",
                    courseId, "Course", cancellationToken);

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

                await PersistNotificationAsync(studentId, "SubmissionGraded",
                    $"Exam Graded — {courseName}",
                    $"Your submission for \"{examTitle}\" has been graded. Score: {score}",
                    cancellationToken: cancellationToken);

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

                await PersistNotificationAsync(studentId, "GradeApproved",
                    $"Grade Approved — {courseName}",
                    $"Your grade for \"{examTitle}\" has been approved.",
                    cancellationToken: cancellationToken);

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

                await PersistGroupNotificationsAsync(courseId, "NewMaterialUploaded",
                    $"New Material — {courseName}",
                    $"New material \"{materialTitle}\" has been uploaded in {courseName}.",
                    courseId, "Course", cancellationToken);

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

                await PersistNotificationAsync(teacherId, "ExamSubmitted",
                    $"Exam Submission — {courseName}",
                    $"{studentName} submitted \"{examTitle}\" in {courseName}.",
                    cancellationToken: cancellationToken);

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

                await PersistNotificationAsync(teacherId, "NewEnrollment",
                    $"New Enrollment — {courseName}",
                    $"{studentName} enrolled in {courseName}.",
                    cancellationToken: cancellationToken);

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

                await PersistNotificationAsync(teacherId, "NewReview",
                    $"New Review — {courseName}",
                    $"A new {rating}-star review was posted on {courseName}.",
                    cancellationToken: cancellationToken);

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

                await PersistNotificationAsync(teacherId, "EnrollmentCompleted",
                    $"Course Completed — {courseName}",
                    $"{studentName} completed {courseName}.",
                    cancellationToken: cancellationToken);

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

                await PersistNotificationAsync(teacherId, "StudentUnenrolled",
                    $"Student Unenrolled — {courseName}",
                    $"{studentName} unenrolled from {courseName}.",
                    cancellationToken: cancellationToken);

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

                await PersistGroupNotificationsAsync(courseId, "NewLectureAdded",
                    $"New Lecture — {courseName}",
                    $"A new lecture \"{lectureTitle}\" has been added to {courseName}.",
                    courseId, "Course", cancellationToken);

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

                await PersistGroupNotificationsAsync(courseId, "CourseUpdated",
                    $"Course Updated — {courseName}",
                    $"{courseName} has been updated.",
                    courseId, "Course", cancellationToken);

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
                await PersistGroupNotificationsAsync(courseId, eventName,
                    isPublished ? $"Course Published — {courseName}" : $"Course Unpublished — {courseName}",
                    isPublished ? $"{courseName} is now published." : $"{courseName} has been unpublished.",
                    courseId, "Course", cancellationToken);

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

                await PersistGroupNotificationsAsync(courseId, "ExamUpdated",
                    $"Exam Updated — {courseName}",
                    $"The exam \"{examTitle}\" in {courseName} has been updated.",
                    courseId, "Course", cancellationToken);

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

                await PersistGroupNotificationsAsync(courseId, "ExamDeleted",
                    $"Exam Removed — {courseName}",
                    $"The exam \"{examTitle}\" in {courseName} has been removed.",
                    courseId, "Course", cancellationToken);

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

                await PersistNotificationAsync(studentId, "GradeUpdated",
                    $"Grade Updated — {courseName}",
                    $"Your grade for \"{examTitle}\" has been updated to {newScore}.",
                    cancellationToken: cancellationToken);

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

                var message = customMessage
                    ?? $"Your teacher has noticed low engagement in {courseName}. Consider reviewing the course materials and completing any pending assignments.";

                // Persist to database so student can see it in notifications
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "EngagementAlert",
                    Title = $"Engagement Alert — {courseName}",
                    Message = message,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Also send real-time via SignalR
                await _studentHubContext.Clients
                    .User(studentId.ToString())
                    .SendAsync("EngagementAlert", new
                    {
                        CourseName = courseName,
                        TeacherName = teacherName,
                        EngagementLevel = engagementLevel,
                        Message = message,
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

        // ─── Cart/Order/Enrollment Notifications ───

        public async Task NotifyCourseAddedToCartAsync(Guid studentId, string courseTitle, Guid courseId, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "CourseAddedToCart",
                    Title = "Course Added",
                    Message = $"{courseTitle} has been added to your cart",
                    RelatedEntityId = courseId,
                    RelatedEntityType = "Course",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create course added to cart notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyCartClearedAsync(Guid studentId, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "CartCleared",
                    Title = "Cart Cleared",
                    Message = "Your shopping cart has been cleared",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create cart cleared notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyCheckoutSuccessAsync(Guid studentId, decimal totalAmount, Guid orderId, int itemCount, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "CheckoutSuccess",
                    Title = "Checkout Successful",
                    Message = $"Your order for {itemCount} course(s) totaling ${totalAmount:F2} has been created",
                    RelatedEntityId = orderId,
                    RelatedEntityType = "Order",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create checkout success notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyPaymentSuccessAsync(Guid studentId, decimal amount, List<string> courseNames, CancellationToken cancellationToken = default)
        {
            try
            {
                var courseList = string.Join(", ", courseNames);
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "PaymentSuccess",
                    Title = "Payment Confirmed",
                    Message = $"Your payment of ${amount:F2} has been confirmed. You are now enrolled in: {courseList}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create payment success notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyUnenrollmentWithRefundAsync(Guid studentId, string courseTitle, decimal refundAmount, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "UnenrollmentWithRefund",
                    Title = "Unenrolled - Refund Issued",
                    Message = $"You have been unenrolled from {courseTitle}. A refund of ${refundAmount:F2} is being processed (May take 3-5 business days)",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create unenrollment with refund notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyUnenrollmentAsync(Guid studentId, string courseTitle, CancellationToken cancellationToken = default)
        {
            try
            {
                var notification = new Notification
                {
                    UserId = studentId,
                    Type = "Unenrollment",
                    Title = "Unenrolled",
                    Message = $"You have been unenrolled from {courseTitle}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Notifications.AddAsync(notification, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create unenrollment notification. StudentId: {StudentId}", studentId);
            }
        }

        public async Task NotifyAIGradingNeedsReviewAsync(
            Guid teacherId,
            string studentName,
            string examTitle,
            Guid submissionId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation(
                    "Notifying teacher about AI grading requiring review. TeacherId: {TeacherId}, Exam: {ExamTitle}",
                    teacherId, examTitle);

                await PersistNotificationAsync(teacherId, "AIGradingReview",
                    "AI Grading Requires Review",
                    $"The submission by {studentName} for \"{examTitle}\" requires your review. Some questions have low confidence scores.",
                    submissionId, "Submission", cancellationToken);

                await _teacherHubContext.Clients
                    .User(teacherId.ToString())
                    .SendAsync("AIGradingReview", new { StudentName = studentName, ExamTitle = examTitle, SubmissionId = submissionId }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send AI grading review notification. TeacherId: {TeacherId}",
                    teacherId);
            }
        }
    }
}