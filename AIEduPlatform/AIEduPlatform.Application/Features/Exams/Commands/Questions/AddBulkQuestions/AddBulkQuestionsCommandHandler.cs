using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddBulkQuestions
{
    public class AddBulkQuestionsCommandHandler : IRequestHandler<AddBulkQuestionsCommand, List<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AddBulkQuestionsCommandHandler> _logger;

        public AddBulkQuestionsCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<AddBulkQuestionsCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<Guid>> Handle(AddBulkQuestionsCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to add questions.");
            }

            // Single query to get exam with course - reduces 2 DB hits to 1
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for bulk add by user {UserId}.", request.ExamId, userId.Value);
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
                _logger.LogWarning("User {UserId} attempted to bulk add questions to exam {ExamId} without permission.", userId.Value, request.ExamId);
                throw new ForbiddenException("You are not authorized to add questions to this exam.");
            }

            _logger.LogInformation(
                "Bulk adding {Count} questions to exam. ExamId: {ExamId}, UserId: {UserId}",
                request.Questions.Count,
                request.ExamId,
                userId.Value);

            try
            {
                var questions = request.Questions.Select(q => new Question
                {
                    ExamId = request.ExamId,
                    Type = q.Type,
                    Text = q.Text,
                    Options = q.Options.Count > 0 ? JsonSerializer.Serialize(q.Options) : "[]",
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points
                }).ToList();

                await _unitOfWork.Questions.AddQuestionsToExamAsync(request.ExamId, questions, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var questionIds = questions.Select(q => q.Id).ToList();

                _logger.LogInformation(
                    "Bulk added {Count} questions to exam. ExamId: {ExamId}, UserId: {UserId}",
                    questions.Count,
                    request.ExamId,
                    userId.Value);

                return questionIds;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error bulk adding questions to exam. ExamId: {ExamId}, UserId: {UserId}",
                    request.ExamId,
                    userId.Value);
                throw;
            }
        }
    }
}
