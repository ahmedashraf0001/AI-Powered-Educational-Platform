using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.ReorderQuestions
{
    public class ReorderQuestionsCommandHandler : IRequestHandler<ReorderQuestionsCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<ReorderQuestionsCommandHandler> _logger;

        public ReorderQuestionsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<ReorderQuestionsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ReorderQuestionsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to reorder questions.");
            }

            // Single query to get exam with course - reduces 2 DB hits to 1
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for reordering questions by user {UserId}.", request.ExamId, userId.Value);
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            var course = exam.Course;

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, request.ExamId);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to reorder questions in exam {ExamId} without permission.", userId.Value, request.ExamId);
                throw new ForbiddenException("You are not authorized to reorder questions in this exam.");
            }

            _logger.LogInformation(
                "Reordering questions in exam. ExamId: {ExamId}, QuestionCount: {Count}, UserId: {UserId}",
                request.ExamId,
                request.QuestionOrders.Count,
                userId.Value);

            try
            {
                await _unitOfWork.Questions.ReorderQuestionsAsync(request.ExamId, request.QuestionOrders, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Questions reordered successfully. ExamId: {ExamId}, UserId: {UserId}",
                    request.ExamId,
                    userId.Value);

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error reordering questions. ExamId: {ExamId}, UserId: {UserId}",
                    request.ExamId,
                    userId.Value);
                throw;
            }
        }
    }
}
