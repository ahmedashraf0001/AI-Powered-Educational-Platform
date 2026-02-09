using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.GenerateQuiz
{
    public class GenerateQuizCommandHandler : IRequestHandler<GenerateQuizCommand, GeneratedQuizDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<GenerateQuizCommandHandler> _logger;

        public GenerateQuizCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<GenerateQuizCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<GeneratedQuizDto> Handle(GenerateQuizCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only generate quizzes in your own study sessions.");

            var ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
            {
                Query = request.Topic,
                CourseId = session.CourseId,
                LectureIds = request.LectureId.HasValue ? [request.LectureId.Value] : null,
                MaterialIds = request.MaterialIds
            }, cancellationToken);

            var questionTypes = request.QuestionTypes
                .Select(qt => qt.ToLowerInvariant() switch
                {
                    "mcq" => QuestionType.MultipleChoice,
                    "true_false" => QuestionType.TrueFalse,
                    "short_answer" => QuestionType.ShortAnswer,
                    "essay" => QuestionType.Essay,
                    _ => QuestionType.MultipleChoice
                })
                .ToList();

            var aiQuestions = await _ollamaClient.GenerateQuizAsync(
                ragResponse.Chunks,
                request.Topic,
                request.NumberOfQuestions,
                request.Difficulty,
                questionTypes,
                cancellationToken);

            var difficulty = Enum.TryParse<QuizDifficulty>(request.Difficulty, ignoreCase: true, out var d)
                ? d
                : QuizDifficulty.Medium;

            var now = DateTime.UtcNow;
            var questionsJson = JsonSerializer.Serialize(aiQuestions);

            var quiz = new GeneratedQuiz
            {
                SessionId = request.SessionId,
                Topic = request.Topic,
                Difficulty = difficulty,
                Questions = questionsJson,
                StudentAnswers = null!,
                Score = 0,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _unitOfWork.GeneratedQuizzes.AddAsync(quiz, cancellationToken);
            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Generated quiz with {Count} questions for session {SessionId}, topic: {Topic}",
                aiQuestions.Count, request.SessionId, request.Topic);

            return new GeneratedQuizDto
            {
                Id = created.Id,
                Topic = created.Topic,
                Difficulty = created.Difficulty,
                Questions = created.Questions,
                StudentAnswers = created.StudentAnswers,
                Score = created.Score,
                CreatedAt = created.CreatedAt
            };
        }
    }
}
