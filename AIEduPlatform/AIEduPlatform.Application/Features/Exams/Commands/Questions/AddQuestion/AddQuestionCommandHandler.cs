using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.Exams.Commands.Questions.AddQuestion
{
    public class AddQuestionCommandHandler : IRequestHandler<AddQuestionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AddQuestionCommandHandler> _logger;

        public AddQuestionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<AddQuestionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Guid> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to add a question.");
            }

            // Single query to get exam with course - reduces 2 DB hits to 1
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for adding question by user {UserId}.", request.ExamId, userId.Value);
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
                _logger.LogWarning("User {UserId} attempted to add question to exam {ExamId} without permission.", userId.Value, request.ExamId);
                throw new ForbiddenException("You are not authorized to add questions to this exam.");
            }

            _logger.LogInformation(
                "Adding question to exam. ExamId: {ExamId}, Type: {Type}, UserId: {UserId}",
                request.ExamId,
                request.Type,
                userId.Value);

            try
            {
                var maxOrder = await _unitOfWork.Questions.GetMaxOrderForExamAsync(request.ExamId, cancellationToken);

                var question = new Question
                {
                    ExamId = request.ExamId,
                    Type = request.Type,
                    Text = request.Text,
                    Options = request.Options.Count > 0 ? JsonSerializer.Serialize(request.Options) : "[]",
                    CorrectAnswer = request.CorrectAnswer,
                    Points = request.Points,
                    Order = maxOrder + 1
                };

                await _unitOfWork.Questions.AddAsync(question, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Question added successfully. QuestionId: {QuestionId}, ExamId: {ExamId}, UserId: {UserId}",
                    question.Id,
                    request.ExamId,
                    userId.Value);

                return question.Id;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(
                    ex,
                    "Error adding question to exam. ExamId: {ExamId}, UserId: {UserId}",
                    request.ExamId,
                    userId.Value);
                throw;
            }
        }
    }
}
