using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Exams;
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

        private const int MaxRetries = 3;

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

            // Scan for submissions with placeholder grades that haven't been AI-graded yet
            await EnqueuePendingAIGradingSubmissionsAsync(stoppingToken);

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

        private async Task EnqueuePendingAIGradingSubmissionsAsync(CancellationToken ct)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                var pending = await unitOfWork.Submissions.GetPendingAIGradingSubmissionsAsync(ct);

                foreach (var submission in pending)
                {
                    var teacherId = submission.Exam?.Course?.TeacherId;
                    if (teacherId.HasValue)
                    {
                        await _queue.EnqueueAsync(new AIGradingRequest(submission.Id, teacherId.Value), ct);
                        _logger.LogInformation(
                            "Enqueued pending AI grading for SubmissionId={SubmissionId}",
                            submission.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue pending AI grading submissions at startup.");
            }
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
            var questionResults = new List<QuestionResultDto>();

            foreach (var question in questions.OrderBy(q => q.Order))
            {
                var questionIdStr = question.Id.ToString();
                var studentAnswer = studentAnswers.GetValueOrDefault(questionIdStr, string.Empty);

                var result = await GradeQuestionWithRetryAsync(
                    question, studentAnswer, contextChunks, ollamaService, ct);

                totalScore += result.Score;
                maxScore += result.MaxScore;

                if (result.RequiresTeacherReview)
                    requiresReview = true;

                if (!string.IsNullOrEmpty(result.Feedback))
                {
                    var formattedScore = result.Score % 1 == 0 ? result.Score.ToString("0") : result.Score.ToString("0.##");
                    feedbackBuilder.AppendLine($"Q{question.Order} | Score: {formattedScore}/{question.Points} | {result.Feedback}");
                }

                questionResults.Add(new QuestionResultDto
                {
                    QuestionId = question.Id,
                    QuestionType = question.Type.ToString(),
                    Score = result.Score,
                    MaxScore = result.MaxScore,
                    Feedback = result.Feedback,
                    IsPartialCredit = result.Score > 0 && result.Score < question.Points,
                    Confidence = result.Confidence,
                    RequiresTeacherReview = result.RequiresTeacherReview
                });
            }

            var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

            // Update existing grade (created by SubmitExamCommandHandler)
            if (submission.Grade != null)
            {
                submission.Grade.Score = percentage;
                submission.Grade.Feedback = feedbackBuilder.ToString();
                submission.Grade.IsAiGraded = true;
                submission.Grade.IsApproved = !requiresReview;
                submission.Grade.QuestionResults = JsonSerializer.Serialize(questionResults);
            }
            else
            {
                var grade = new Grade
                {
                    SubmissionId = request.SubmissionId,
                    Score = percentage,
                    Feedback = feedbackBuilder.ToString(),
                    IsAiGraded = true,
                    IsApproved = !requiresReview,
                    QuestionResults = JsonSerializer.Serialize(questionResults)
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

        private async Task<QuestionGradeResult> GradeQuestionWithRetryAsync(
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

                return new QuestionGradeResult
                {
                    Score = isCorrect ? question.Points : 0,
                    MaxScore = question.Points,
                    Feedback = isCorrect ? "Correct!" : $"Incorrect. The correct answer is: {question.CorrectAnswer}",
                    Confidence = 1.0f,
                    RequiresTeacherReview = false
                };
            }

            if (question.Type == QuestionType.FillInTheBlank)
            {
                var isCorrect = string.Equals(
                    studentAnswer.Trim(),
                    question.CorrectAnswer?.Trim() ?? "",
                    StringComparison.OrdinalIgnoreCase);

                return new QuestionGradeResult
                {
                    Score = isCorrect ? question.Points : 0,
                    MaxScore = question.Points,
                    Feedback = isCorrect ? "Correct!" : $"Incorrect. Expected: {question.CorrectAnswer}",
                    Confidence = 1.0f,
                    RequiresTeacherReview = false
                };
            }

            if (question.Type is QuestionType.Essay or QuestionType.ShortAnswer)
            {
                if (string.IsNullOrWhiteSpace(studentAnswer))
                {
                    return new QuestionGradeResult
                    {
                        Score = 0,
                        MaxScore = question.Points,
                        Feedback = "No answer provided.",
                        Confidence = 1.0f,
                        RequiresTeacherReview = false
                    };
                }

                var modelAnswer = question.ModelAnswer ?? question.CorrectAnswer;

                for (var attempt = 1; attempt <= MaxRetries; attempt++)
                {
                    try
                    {
                        var essayGrade = await ollamaService.GradeEssayAsync(
                            contextChunks,
                            question.Text,
                            question.Points,
                            modelAnswer,
                            studentAnswer,
                            ct);

                        var scaledScore = essayGrade.MaxPoints > 0
                            ? (essayGrade.Score / essayGrade.MaxPoints) * question.Points
                            : 0f;

                        var roundedScore = NormalizeWrittenScore(scaledScore, question.Points);
                        var listFloorScore = CalculateListStylePartialFloor(question, modelAnswer, studentAnswer);
                        var finalScore = Math.Max(roundedScore, listFloorScore);

                        var requiresTeacherReview = essayGrade.RequiresTeacherReview || essayGrade.Confidence < 0.5f;

                        var feedback = essayGrade.Feedback;
                        if (listFloorScore > roundedScore + 0.001f)
                        {
                            feedback = string.IsNullOrWhiteSpace(feedback)
                                ? "Partial credit awarded for matching expected items."
                                : $"{feedback} Partial credit awarded for matching expected items.";
                        }

                        return new QuestionGradeResult
                        {
                            Score = finalScore,
                            MaxScore = question.Points,
                            Feedback = feedback,
                            Confidence = essayGrade.Confidence,
                            RequiresTeacherReview = requiresTeacherReview
                        };
                    }
                    catch (Exception ex) when (attempt < MaxRetries)
                    {
                        _logger.LogWarning(
                            ex,
                            "AI grading failed for question {QuestionId} (attempt {Attempt}/{MaxRetries}). Retrying...",
                            question.Id, attempt, MaxRetries);

                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(
                            ex,
                            "AI grading failed for question {QuestionId} after {MaxRetries} attempts. Marking for teacher review.",
                            question.Id, MaxRetries);
                    }
                }

                return new QuestionGradeResult
                {
                    Score = 0f,
                    MaxScore = question.Points,
                    Feedback = "AI grading is currently unavailable. Teacher review is required.",
                    Confidence = 0f,
                    RequiresTeacherReview = true
                };
            }

            return new QuestionGradeResult
            {
                Score = 0,
                MaxScore = question.Points,
                Feedback = "Unknown question type.",
                Confidence = 0f,
                RequiresTeacherReview = true
            };
        }

        private class QuestionGradeResult
        {
            public float Score { get; init; }
            public float MaxScore { get; init; }
            public string Feedback { get; init; } = string.Empty;
            public float Confidence { get; init; }
            public bool RequiresTeacherReview { get; init; }
        }

        private static float NormalizeWrittenScore(float rawScore, float maxScore)
        {
            var clamped = Math.Clamp(rawScore, 0f, maxScore);
            var rounded = (float)Math.Round(clamped * 2f, MidpointRounding.AwayFromZero) / 2f;
            return Math.Clamp(rounded, 0f, maxScore);
        }

        private static float CalculateListStylePartialFloor(Question question, string? modelAnswer, string studentAnswer)
        {
            if (string.IsNullOrWhiteSpace(studentAnswer) || string.IsNullOrWhiteSpace(modelAnswer))
            {
                return 0f;
            }

            var requiredCount = ExtractRequiredItemCount(question.Text);
            if (requiredCount <= 1)
            {
                return 0f;
            }

            var expectedItems = SplitExpectedItems(modelAnswer);
            if (expectedItems.Count < 2)
            {
                return 0f;
            }

            var studentItems = SplitExpectedItems(studentAnswer);
            if (studentItems.Count == 0)
            {
                return 0f;
            }

            var matched = 0;
            var usedStudentIndices = new HashSet<int>();

            foreach (var expected in expectedItems)
            {
                for (var i = 0; i < studentItems.Count; i++)
                {
                    if (usedStudentIndices.Contains(i))
                    {
                        continue;
                    }

                    if (!AreItemsEquivalent(expected, studentItems[i]))
                    {
                        continue;
                    }

                    matched++;
                    usedStudentIndices.Add(i);
                    break;
                }
            }

            if (matched <= 0)
            {
                return 0f;
            }

            var denominator = Math.Min(requiredCount, expectedItems.Count);
            if (denominator <= 0)
            {
                return 0f;
            }

            var ratio = Math.Clamp(matched / (float)denominator, 0f, 1f);
            return NormalizeWrittenScore(question.Points * ratio, question.Points);
        }

        private static int ExtractRequiredItemCount(string? questionText)
        {
            if (string.IsNullOrWhiteSpace(questionText))
            {
                return 0;
            }

            var normalized = questionText.ToLowerInvariant();

            var digitMatch = Regex.Match(normalized, @"\b(\d+)\b");
            if (digitMatch.Success && int.TryParse(digitMatch.Groups[1].Value, out var value) && value > 1)
            {
                return value;
            }

            if (Regex.IsMatch(normalized, @"\btwo\b")) return 2;
            if (Regex.IsMatch(normalized, @"\bthree\b")) return 3;
            if (Regex.IsMatch(normalized, @"\bfour\b")) return 4;
            if (Regex.IsMatch(normalized, @"\bfive\b")) return 5;

            return 0;
        }

        private static List<string> SplitExpectedItems(string value)
        {
            return value
                .ToLowerInvariant()
                .Replace("\r", "\n")
                .Replace("&", ",")
                .Replace(" and ", ",")
                .Replace("/", ",")
                .Split(new[] { ',', ';', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => Regex.Replace(item, @"\s+", " ").Trim())
                .Where(item => item.Length > 1)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private static bool AreItemsEquivalent(string expected, string actual)
        {
            if (string.Equals(expected, actual, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (actual.Contains(expected, StringComparison.OrdinalIgnoreCase) ||
                expected.Contains(actual, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var distance = ComputeLevenshteinDistance(expected, actual);
            return distance <= 1;
        }

        private static int ComputeLevenshteinDistance(string a, string b)
        {
            if (a.Length == 0) return b.Length;
            if (b.Length == 0) return a.Length;

            var matrix = new int[a.Length + 1, b.Length + 1];

            for (var i = 0; i <= a.Length; i++) matrix[i, 0] = i;
            for (var j = 0; j <= b.Length; j++) matrix[0, j] = j;

            for (var i = 1; i <= a.Length; i++)
            {
                for (var j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(
                        Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[a.Length, b.Length];
        }
    }
}
