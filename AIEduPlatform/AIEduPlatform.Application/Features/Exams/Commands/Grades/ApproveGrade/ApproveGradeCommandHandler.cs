using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.ApproveGrade
{
    public class ApproveGradeCommandHandler : IRequestHandler<ApproveGradeCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ApproveGradeCommandHandler> _logger;

        public ApproveGradeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<ApproveGradeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ApproveGradeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to approve a grade.");
            }

            // Single query to get grade with submission, exam, and course - reduces 4-5 DB hits to 1
            var grade = await _unitOfWork.Grades.GetGradeWithSubmissionExamAndCourseAsync(request.GradeId, cancellationToken);

            if (grade == null)
            {
                _logger.LogWarning("Grade {GradeId} not found for approval by user {UserId}.", request.GradeId, userId.Value);
                throw new NotFoundException(nameof(Grade), request.GradeId);
            }

            if (grade.IsApproved)
            {
                _logger.LogWarning("Grade {GradeId} has already been approved.", request.GradeId);
                throw new BadRequestException("This grade has already been approved.");
            }

            if (!grade.IsAiGraded)
            {
                _logger.LogWarning("Grade {GradeId} is not AI-graded and cannot be approved.", request.GradeId);
                throw new BadRequestException("Only AI-graded submissions can be approved.");
            }

            var submission = grade.Submission;

            if (submission == null)
            {
                _logger.LogWarning("Submission not found for grade {GradeId}.", request.GradeId);
                throw new NotFoundException(nameof(Submission), grade.SubmissionId);
            }

            var exam = submission.Exam;

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for submission {SubmissionId}.", submission.ExamId, submission.Id);
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
                _logger.LogWarning("User {UserId} attempted to approve grade {GradeId} without permission.", userId.Value, request.GradeId);
                throw new ForbiddenException("You are not authorized to approve this grade.");
            }

            _logger.LogInformation(
                "Approving AI-generated grade. GradeId: {GradeId}, SubmissionId: {SubmissionId}, TeacherId: {TeacherId}",
                request.GradeId,
                grade.SubmissionId,
                userId.Value);

            try
            {
                await _unitOfWork.Grades.ApproveGradeAsync(request.GradeId, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Grade approved successfully. GradeId: {GradeId}, SubmissionId: {SubmissionId}",
                    request.GradeId,
                    grade.SubmissionId);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error approving grade. GradeId: {GradeId}, TeacherId: {TeacherId}",
                    request.GradeId,
                    userId.Value);
                throw;
            }
        }
    }
}
