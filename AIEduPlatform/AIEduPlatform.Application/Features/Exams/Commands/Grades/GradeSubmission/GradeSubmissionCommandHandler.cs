using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission
{
    public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<GradeSubmissionCommandHandler> _logger;

        public GradeSubmissionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<GradeSubmissionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<Guid> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to grade a submission.");
            }

            // Single query to get submission with exam and course - reduces 3-4 DB hits to 1
            var submission = await _unitOfWork.Submissions.GetSubmissionWithExamAndCourseAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found for grading by user {UserId}.", request.SubmissionId, userId.Value);
                throw new NotFoundException(nameof(Submission), request.SubmissionId);
            }

            if (submission.Grade != null)
            {
                _logger.LogWarning("Submission {SubmissionId} has already been graded.", request.SubmissionId);
                throw new BadRequestException("This submission has already been graded. Use update grade if you want to modify it.");
            }

            var exam = submission.Exam;

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for submission {SubmissionId}.", submission.ExamId, request.SubmissionId);
                throw new NotFoundException(nameof(Exam), submission.ExamId);
            }

            var course = exam.Course;

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, exam.Id);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to grade submission {SubmissionId} without permission.", userId.Value, request.SubmissionId);
                throw new ForbiddenException("You are not authorized to grade this submission.");
            }

            _logger.LogInformation(
                "Grading submission. SubmissionId: {SubmissionId}, Score: {Score}, TeacherId: {TeacherId}",
                request.SubmissionId,
                request.Score,
                userId.Value);

            try
            {
                var grade = new Grade
                {
                    SubmissionId = request.SubmissionId,
                    Score = request.Score,
                    Feedback = request.Feedback,
                    IsAiGraded = false,
                    IsApproved = true
                };

                await _unitOfWork.Grades.AddAsync(grade, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Submission graded successfully. GradeId: {GradeId}, SubmissionId: {SubmissionId}, Score: {Score}",
                    grade.Id,
                    request.SubmissionId,
                    request.Score);

                await _notificationService.NotifySubmissionGradedAsync(
                    submission.StudentId, course.Title, exam.Title, (decimal)request.Score, cancellationToken);

                await _auditService.LogGradeActionAsync(
                    userId.Value, "ManualGrade", request.SubmissionId, grade.Id,
                    $"Score: {request.Score}", cancellationToken);

                return grade.Id;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error grading submission. SubmissionId: {SubmissionId}, TeacherId: {TeacherId}",
                    request.SubmissionId,
                    userId.Value);
                throw;
            }
        }
    }
}
