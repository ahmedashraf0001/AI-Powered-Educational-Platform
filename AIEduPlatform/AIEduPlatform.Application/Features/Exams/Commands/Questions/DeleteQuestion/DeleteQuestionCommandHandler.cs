using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.DeleteQuestion
{
    public class DeleteQuestionCommandHandler : IRequestHandler<DeleteQuestionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteQuestionCommandHandler> _logger;

        public DeleteQuestionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<DeleteQuestionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteQuestionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to delete a question.");
            }

            // Single query to get question with exam and course - reduces 3 DB hits to 1
            var questionWithRelations = await _unitOfWork.Questions.GetQuestionWithExamAndCourseAsync(request.QuestionId, cancellationToken);

            if (questionWithRelations == null)
            {
                _logger.LogWarning("Question {QuestionId} not found for deletion by user {UserId}.", request.QuestionId, userId.Value);
                throw new NotFoundException(nameof(Question), request.QuestionId);
            }

            var exam = questionWithRelations.Exam;

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for question {QuestionId}.", questionWithRelations.ExamId, request.QuestionId);
                throw new NotFoundException(nameof(Exam), questionWithRelations.ExamId);
            }

            var course = exam.Course;

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, exam.Id);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to delete question {QuestionId} without permission.", userId.Value, request.QuestionId);
                throw new ForbiddenException("You are not authorized to delete this question.");
            }

            _logger.LogInformation(
                "Deleting question. QuestionId: {QuestionId}, ExamId: {ExamId}, UserId: {UserId}",
                request.QuestionId,
                questionWithRelations.ExamId,
                userId.Value);

            try
            {
                // Re-fetch for tracking since we used AsNoTracking
                var question = await _unitOfWork.Questions.GetByIdAsync(request.QuestionId, cancellationToken);
                await _unitOfWork.Questions.DeleteAsync(question!, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Question deleted successfully. QuestionId: {QuestionId}, UserId: {UserId}",
                    request.QuestionId,
                    userId.Value);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error deleting question. QuestionId: {QuestionId}, UserId: {UserId}",
                    request.QuestionId,
                    userId.Value);
                throw;
            }
        }
    }
}
