using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam
{
    public class SubmitExamCommandHandler : IRequestHandler<SubmitExamCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<SubmitExamCommandHandler> _logger;

        public SubmitExamCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<SubmitExamCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Guid> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to submit an exam.");
            }

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for submission by user {UserId}.", request.ExamId, userId.Value);
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            var now = DateTime.UtcNow;
            if (now < exam.StartTime)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} before start time.", userId.Value, request.ExamId);
                throw new BadRequestException("The exam has not started yet.");
            }

            if (now > exam.EndTime)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} after end time.", userId.Value, request.ExamId);
                throw new BadRequestException("The exam has ended and is no longer accepting submissions.");
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(exam.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, request.ExamId);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(exam.CourseId, userId.Value, cancellationToken);

            if (!isEnrolled)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} without being enrolled in course {CourseId}.", userId.Value, request.ExamId, exam.CourseId);
                throw new ForbiddenException("You must be enrolled in the course to submit this exam.");
            }

            var existingSubmission = await _unitOfWork.Submissions.GetSubmissionByExamAndStudentAsync(
                request.ExamId,
                userId.Value,
                false,
                cancellationToken);

            if (existingSubmission != null)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} multiple times.", userId.Value, request.ExamId);
                throw new BadRequestException("You have already submitted this exam.");
            }

            _logger.LogInformation(
                "Submitting exam. ExamId: {ExamId}, StudentId: {StudentId}",
                request.ExamId,
                userId.Value);

            try
            {
                var submission = new Submission
                {
                    ExamId = request.ExamId,
                    StudentId = userId.Value,
                    Answers = JsonSerializer.Serialize(request.Answers),
                    SubmittedAt = DateTime.UtcNow
                };

                await _unitOfWork.Submissions.AddAsync(submission, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Exam submitted successfully. SubmissionId: {SubmissionId}, ExamId: {ExamId}, StudentId: {StudentId}",
                    submission.Id,
                    request.ExamId,
                    userId.Value);

                return submission.Id;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error submitting exam. ExamId: {ExamId}, StudentId: {StudentId}",
                    request.ExamId,
                    userId.Value);
                throw;
            }
        }
    }
}
