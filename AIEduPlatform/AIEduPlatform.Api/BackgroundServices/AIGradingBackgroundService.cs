using System.Text;
using System.Text.Json;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.DTOs.RAG.Context;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Api.BackgroundServices
{
    public class AIGradingBackgroundService : BackgroundService
    {
        private readonly IAIGradingQueue _queue;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AIGradingBackgroundService> _logger;

        public AIGradingBackgroundService(
            IAIGradingQueue queue,
            IServiceProvider serviceProvider,
            ILogger<AIGradingBackgroundService> logger)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AIGradingBackgroundService started.");

            await foreach (var request in _queue.DequeueAllAsync(stoppingToken))
            {
                try
                {
                    _logger.LogInformation(
                        "Processing AI grading request for SubmissionId={SubmissionId}",
                        request.SubmissionId);

                    using var scope = _serviceProvider.CreateScope();
                    await ProcessGradingAsync(scope.ServiceProvider, request, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    _logger.LogWarning("AI grading operation cancelled for SubmissionId={SubmissionId}", request.SubmissionId);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error processing AI grading request for SubmissionId={SubmissionId}",
                        request.SubmissionId);
                }
            }

            _logger.LogInformation("AIGradingBackgroundService stopped.");
        }

        private async Task ProcessGradingAsync(
            IServiceProvider services,
            AIGradingRequest request,
            CancellationToken ct)
        {
            var unitOfWork = services.GetRequiredService<IUnitOfWork>();
            var ragService = services.GetRequiredService<IRAGService>();
            var ollamaService = services.GetRequiredService<IOllamaServiceClient>();
            var notificationService = services.GetRequiredService<INotificationService>();

            var submission = await unitOfWork.Submissions.GetSubmissionWithExamAndCourseAsync(request.SubmissionId, ct);
            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found for AI grading.", request.SubmissionId);
                return;
            }

            var exam = submission.Exam;
            var course = exam?.Course;

            if (exam == null || course == null)
            {
                _logger.LogWarning("Exam or Course not found for submission {SubmissionId}.", request.SubmissionId);
                return;
            }

            var questions = await unitOfWork.Questions.GetQuestionsByExamIdAsync(exam.Id, ct);
            if (questions == null || !questions.Any())
            {
                _logger.LogWarning("No questions found for exam {ExamId}.", exam.Id);
                return;
            }

            Dictionary<string, string> studentAnswers;
            try
            {
                studentAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(submission.Answers)
                    ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                _logger.LogWarning("Failed to parse student answers for submission {SubmissionId}.", request.SubmissionId);
                return;
            }

            // Retrieve course content context for essay grading
            var ragRequest = new RagRetrievalRequest
            {
                Query = $"Course content for grading exam answers in {course.Title}",
                CourseId = course.Id,
                TopK = 20,
                FinalTopK = 10,
                MinScore = 0.2f,
                UseReranking = true
            };

            var ragResponse = await ragService.RetrieveAsync(ragRequest, ct);
            var contextChunks = ragResponse.Success ? ragResponse.Chunks : new List<ContextChunk>();

            float totalScore = 0;
            float maxScore = 0;
            bool requiresReview = false;
            var feedbackBuilder = new StringBuilder();

            foreach (var question in questions.OrderBy(q => q.Order))
            {
                var questionIdStr = question.Id.ToString();
                var studentAnswer = studentAnswers.GetValueOrDefault(questionIdStr, string.Empty);

                var (score, qMaxScore, feedback, confidence) = await GradeQuestionAsync(
                    question, studentAnswer, contextChunks, ollamaService, ct);

                totalScore += score;
                maxScore += qMaxScore;

                if (confidence < 0.7f)
                    requiresReview = true;

                if (!string.IsNullOrEmpty(feedback))
                    feedbackBuilder.AppendLine($"Q{question.Order}: {feedback}");
            }

            var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

            // Create or update grade
            if (submission.Grade != null)
            {
                submission.Grade.Score = percentage;
                submission.Grade.Feedback = feedbackBuilder.ToString();
                submission.Grade.IsAiGraded = true;
                submission.Grade.IsApproved = !requiresReview;
            }
            else
            {
                var grade = new Grade
                {
                    SubmissionId = request.SubmissionId,
                    Score = percentage,
                    Feedback = feedbackBuilder.ToString(),
                    IsAiGraded = true,
                    IsApproved = !requiresReview
                };
                await unitOfWork.Grades.AddAsync(grade, ct);
            }

            await unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation(
                "AI grading completed. SubmissionId: {SubmissionId}, Score: {Score}%, RequiresReview: {RequiresReview}",
                request.SubmissionId, percentage, requiresReview);

            // Notify student
            await notificationService.NotifySubmissionGradedAsync(
                submission.StudentId, course.Title, exam.Title, (decimal)percentage, ct);

            // Notify teacher if requires review
            if (requiresReview)
            {
                var student = await unitOfWork.Users.GetUserByIdAsync(submission.StudentId, ct: ct);
                await notificationService.NotifyAIGradingNeedsReviewAsync(
                    request.TeacherId,
                    student?.FirstName ?? "a student",
                    exam.Title,
                    request.SubmissionId,
                    ct);
            }
        }

        private async Task<(float score, float maxScore, string feedback, float confidence)> GradeQuestionAsync(
            Question question,
            string studentAnswer,
            List<ContextChunk> contextChunks,
            IOllamaServiceClient ollamaService,
            CancellationToken ct)
        {
            if (question.Type is QuestionType.MultipleChoice or QuestionType.TrueFalse)
            {
                var isCorrect = string.Equals(
                    studentAnswer.Trim(),
                    question.CorrectAnswer?.Trim() ?? "",
                    StringComparison.OrdinalIgnoreCase);

                return (
                    isCorrect ? question.Points : 0,
                    question.Points,
                    isCorrect ? "Correct!" : $"Incorrect. The correct answer is: {question.CorrectAnswer}",
                    1.0f
                );
            }

            if (question.Type == QuestionType.FillInTheBlank)
            {
                var isCorrect = string.Equals(
                    studentAnswer.Trim(),
                    question.CorrectAnswer?.Trim() ?? "",
                    StringComparison.OrdinalIgnoreCase);

                return (
                    isCorrect ? question.Points : 0,
                    question.Points,
                    isCorrect ? "Correct!" : $"Incorrect. Expected: {question.CorrectAnswer}",
                    1.0f
                );
            }

            if (question.Type is QuestionType.Essay or QuestionType.ShortAnswer)
            {
                if (string.IsNullOrWhiteSpace(studentAnswer))
                {
                    return (0, question.Points, "No answer provided.", 1.0f);
                }

                try
                {
                    var modelAnswer = question.ModelAnswer ?? question.CorrectAnswer;
                    var essayGrade = await ollamaService.GradeEssayAsync(
                        contextChunks,
                        question.Text,
                        question.Points,
                        modelAnswer,
                        studentAnswer,
                        ct);

                    var scaledScore = (essayGrade.Score / essayGrade.MaxPoints) * question.Points;
                    return (scaledScore, question.Points, essayGrade.Feedback, essayGrade.Confidence);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "AI grading failed for question {QuestionId}.", question.Id);
                    return (0, question.Points, "AI grading failed. Please review manually.", 0f);
                }
            }

            return (0, question.Points, "Unknown question type.", 0f);
        }
    }
}
