using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.UpdateGrade
{
    public class UpdateGradeCommandHandler : IRequestHandler<UpdateGradeCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateGradeCommandHandler> _logger;

        public UpdateGradeCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateGradeCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateGradeCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to update a grade.");
            }

            // Single query to get grade with submission, exam, and course - reduces 4-5 DB hits to 1
            var grade = await _unitOfWork.Grades.GetGradeWithSubmissionExamAndCourseAsync(request.GradeId, cancellationToken);

            if (grade == null)
            {
                _logger.LogWarning("Grade {GradeId} not found for update by user {UserId}.", request.GradeId, userId.Value);
                throw new NotFoundException(nameof(Grade), request.GradeId);
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
                _logger.LogWarning("User {UserId} attempted to update grade {GradeId} without permission.", userId.Value, request.GradeId);
                throw new ForbiddenException("You are not authorized to update this grade.");
            }

            _logger.LogInformation(
                "Updating grade. GradeId: {GradeId}, OldScore: {OldScore}, NewScore: {NewScore}, TeacherId: {TeacherId}",
                request.GradeId,
                grade.Score,
                request.Score,
                userId.Value);

            try
            {
                await _unitOfWork.Grades.UpdateGradeAsync(request.GradeId, request.Score, request.Feedback, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Grade updated successfully. GradeId: {GradeId}, NewScore: {Score}",
                    request.GradeId,
                    request.Score);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error updating grade. GradeId: {GradeId}, TeacherId: {TeacherId}",
                    request.GradeId,
                    userId.Value);
                throw;
            }
        }
    }
}
