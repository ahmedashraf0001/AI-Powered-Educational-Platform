using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.AI.Simple;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Quizzes.SubmitQuizAnswers
{
    public class SubmitQuizAnswersCommandHandler : IRequestHandler<SubmitQuizAnswersCommand, QuizResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IOllamaServiceClient _ollamaClient;
        private readonly IRAGService _ragService;
        private readonly ILogger<SubmitQuizAnswersCommandHandler> _logger;

        private static readonly HashSet<string> WrittenTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "short_answer", "essay"
        };

        public SubmitQuizAnswersCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IOllamaServiceClient ollamaClient,
            IRAGService ragService,
            ILogger<SubmitQuizAnswersCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ollamaClient = ollamaClient;
            _ragService = ragService;
            _logger = logger;
        }

        public async Task<QuizResultDto> Handle(SubmitQuizAnswersCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var quiz = await _unitOfWork.GeneratedQuizzes.GetByIdWithSessionAsync(request.QuizId, cancellationToken);
            if (quiz is null)
                throw new NotFoundException(nameof(GeneratedQuiz), request.QuizId);
            if (quiz.Session.StudentId != userId.Value)
                throw new ForbiddenException("You can only submit answers for your own quizzes.");
            if (quiz.SessionId != request.SessionId)
                throw new BadRequestException("Quiz does not belong to the specified session.");

            var questions = JsonSerializer.Deserialize<List<QuizQuestion>>(quiz.Questions)
                ?? throw new BadRequestException("Failed to parse quiz questions.");

            var hasWrittenQuestions = questions.Any(q => WrittenTypes.Contains(q.QuestionType));
            RagRetrievalResponse? ragResponse = null;

            if (hasWrittenQuestions)
            {
                ragResponse = await _ragService.RetrieveAsync(new RagRetrievalRequest
                {
                    Query = quiz.Topic,
                    CourseId = quiz.Session.CourseId
                }, cancellationToken);
            }

            var results = new List<QuizAnswerResultDto>();
            float totalScore = 0;
            int correctCount = 0;

            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var studentAnswer = request.Answers.GetValueOrDefault(i, string.Empty);

                if (WrittenTypes.Contains(question.QuestionType))
                {
                    var maxPoints = question.SuggestedPoints > 0 ? question.SuggestedPoints : 10;

                    var grade = await _ollamaClient.GradeEssayAsync(
                        ragResponse!.Chunks,
                        question.QuestionText,
                        maxPoints,
                        question.CorrectAnswer,
                        studentAnswer,
                        cancellationToken);

                    var finalPercentage = Math.Clamp(grade.Percentage, 0f, 100f);

                    var isPass = finalPercentage >= 50f;
                    if (isPass) correctCount++;
                    totalScore += finalPercentage;

                    results.Add(new QuizAnswerResultDto
                    {
                        QuestionIndex = i,
                        StudentAnswer = studentAnswer,
                        CorrectAnswer = question.CorrectAnswer,
                        IsCorrect = isPass,
                        Explanation = question.Explanation,
                        AiScore = finalPercentage,
                        AiFeedback = grade.Feedback
                    });
                }
                else
                {
                    var isCorrect = string.Equals(
                        studentAnswer.Trim(),
                        question.CorrectAnswer.Trim(),
                        StringComparison.OrdinalIgnoreCase);

                    if (isCorrect) correctCount++;
                    totalScore += isCorrect ? 100f : 0f;

                    results.Add(new QuizAnswerResultDto
                    {
                        QuestionIndex = i,
                        StudentAnswer = studentAnswer,
                        CorrectAnswer = question.CorrectAnswer,
                        IsCorrect = isCorrect,
                        Explanation = question.Explanation
                    });
                }
            }

            var score = questions.Count > 0 ? totalScore / questions.Count : 0;

            quiz.StudentAnswers = JsonSerializer.Serialize(request.Answers);
            quiz.Score = score;
            quiz.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.GeneratedQuizzes.Update(quiz);

            await _unitOfWork.StudySessions.UpdateLastActivityAsync(request.SessionId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Quiz {QuizId} submitted: {Correct}/{Total} correct ({Score}%)",
                quiz.Id, correctCount, questions.Count, score);

            return new QuizResultDto
            {
                QuizId = quiz.Id,
                Score = score,
                TotalQuestions = questions.Count,
                CorrectCount = correctCount,
                Results = results
            };
        }
    }
}
