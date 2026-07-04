using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.DTOs.RAG;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmissionWithAI
{
    public class GradeSubmissionWithAICommandHandler : IRequestHandler<GradeSubmissionWithAICommand, GradeSubmissionWithAIResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRAGService _ragService;
        private readonly IOllamaServiceClient _ollamaService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<GradeSubmissionWithAICommandHandler> _logger;

        public GradeSubmissionWithAICommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IRAGService ragService,
            IOllamaServiceClient ollamaService,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<GradeSubmissionWithAICommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _ragService = ragService;
            _ollamaService = ollamaService;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<GradeSubmissionWithAIResult> Handle(GradeSubmissionWithAICommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to grade a submission.");
            }

            var submission = await _unitOfWork.Submissions.GetSubmissionWithExamAndCourseAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found for AI grading by user {UserId}.", request.SubmissionId, userId.Value);
                throw new NotFoundException(nameof(Submission), request.SubmissionId);
            }

            if (submission.Grade != null)
            {
                _logger.LogInformation("Submission {SubmissionId} already has a grade. It will be updated by AI grading.", request.SubmissionId);
            }

            var exam = submission.Exam;

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for submission {SubmissionId}.", submission.ExamId, request.SubmissionId);
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
                _logger.LogWarning("User {UserId} attempted to AI grade submission {SubmissionId} without permission.", userId.Value, request.SubmissionId);
                throw new ForbiddenException("You are not authorized to grade this submission.");
            }

            _logger.LogInformation(
                "Starting AI grading for submission. SubmissionId: {SubmissionId}, ExamId: {ExamId}, TeacherId: {TeacherId}",
                request.SubmissionId,
                exam.Id,
                userId.Value);

            try
            {
                var questions = await _unitOfWork.Questions.GetQuestionsByExamIdAsync(exam.Id, cancellationToken);

                if (questions == null || !questions.Any())
                {
                    return new GradeSubmissionWithAIResult
                    {
                        Success = false,
                        Error = "No questions found for this exam."
                    };
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
                    return new GradeSubmissionWithAIResult
                    {
                        Success = false,
                        Error = "Invalid answer format in submission."
                    };
                }

                var ragRequest = new RagRetrievalRequest
                {
                    Query = $"Course content for grading exam answers in {course.Title}",
                    CourseId = course.Id,
                    TopK = 20,
                    FinalTopK = 10,
                    MinScore = 0.2f,
                    UseReranking = true
                };

                var ragResponse = await _ragService.RetrieveAsync(ragRequest, cancellationToken);
                var contextChunks = ragResponse.Success ? ragResponse.Chunks : new();

                var questionGrades = new List<QuestionGradeDetail>();
                var questionResults = new List<QuestionResultDto>();
                float totalScore = 0;
                float maxScore = 0;
                bool requiresReview = false;
                var feedbackBuilder = new StringBuilder();

                foreach (var question in questions.OrderBy(q => q.Order))
                {
                    var questionIdStr = question.Id.ToString();
                    var studentAnswer = studentAnswers.GetValueOrDefault(questionIdStr, string.Empty);

                    var gradeDetail = await GradeQuestionAsync(
                        question,
                        studentAnswer,
                        contextChunks,
                        cancellationToken);

                    questionGrades.Add(gradeDetail);
                    totalScore += gradeDetail.Score;
                    maxScore += gradeDetail.MaxScore;

                    if (gradeDetail.RequiresTeacherReview)
                    {
                        requiresReview = true;
                    }

                    if (!string.IsNullOrEmpty(gradeDetail.Feedback))
                    {
                        var formattedScore = gradeDetail.Score % 1 == 0 ? gradeDetail.Score.ToString("0") : gradeDetail.Score.ToString("0.##");
                        feedbackBuilder.AppendLine($"Q{question.Order} | Score: {formattedScore}/{question.Points} | {gradeDetail.Feedback}");
                    }

                    questionResults.Add(new QuestionResultDto
                    {
                        QuestionId = question.Id,
                        QuestionType = question.Type.ToString(),
                        Score = gradeDetail.Score,
                        MaxScore = gradeDetail.MaxScore,
                        Feedback = gradeDetail.Feedback,
                        IsPartialCredit = gradeDetail.IsPartialCredit,
                        Confidence = gradeDetail.Confidence,
                        RequiresTeacherReview = gradeDetail.RequiresTeacherReview
                    });
                }

                var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

                Grade grade;
                if (submission.Grade != null)
                {
                    // Update existing grade (from auto-grading or a previous attempt)
                    submission.Grade.Score = percentage;
                    submission.Grade.Feedback = feedbackBuilder.ToString();
                    submission.Grade.IsAiGraded = true;
                    submission.Grade.IsApproved = !requiresReview;
                    submission.Grade.QuestionResults = JsonSerializer.Serialize(questionResults);
                    grade = submission.Grade;
                }
                else
                {
                    grade = new Grade
                    {
                        SubmissionId = request.SubmissionId,
                        Score = percentage,
                        Feedback = feedbackBuilder.ToString(),
                        IsAiGraded = true,
                        IsApproved = !requiresReview,
                        QuestionResults = JsonSerializer.Serialize(questionResults)
                    };
                    await _unitOfWork.Grades.AddAsync(grade, CancellationToken.None);
                }
                await _unitOfWork.SaveChangesAsync(CancellationToken.None);

                _logger.LogInformation(
                    "AI grading completed. GradeId: {GradeId}, SubmissionId: {SubmissionId}, Score: {Score}/{MaxScore} ({Percentage}%)",
                    grade.Id,
                    request.SubmissionId,
                    totalScore,
                    maxScore,
                    percentage);

                await _notificationService.NotifySubmissionGradedAsync(
                    submission.StudentId, course!.Title, exam!.Title, (decimal)percentage, cancellationToken);

                await _auditService.LogGradeActionAsync(
                    userId.Value, "AIGrade", request.SubmissionId, grade.Id,
                    $"Score: {percentage:F1}% ({totalScore}/{maxScore}), RequiresReview: {requiresReview}", cancellationToken);

                return new GradeSubmissionWithAIResult
                {
                    Success = true,
                    GradeId = grade.Id,
                    TotalScore = totalScore,
                    MaxScore = maxScore,
                    Percentage = percentage,
                    Feedback = feedbackBuilder.ToString(),
                    RequiresTeacherReview = requiresReview,
                    QuestionGrades = questionGrades
                };
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error during AI grading for submission {SubmissionId}. TeacherId: {TeacherId}",
                    request.SubmissionId,
                    userId.Value);

                return new GradeSubmissionWithAIResult
                {
                    Success = false,
                    Error = "An error occurred while grading the submission. Please try again."
                };
            }
        }

        private async Task<QuestionGradeDetail> GradeQuestionAsync(
            Question question,
            string studentAnswer,
            List<Core.DTOs.RAG.Context.ContextChunk> contextChunks,
            CancellationToken cancellationToken)
        {
            if (question.Type is QuestionType.MultipleChoice or QuestionType.TrueFalse)
            {
                var isCorrect = string.Equals(
                    studentAnswer.Trim(),
                    question.CorrectAnswer.Trim(),
                    StringComparison.OrdinalIgnoreCase);

                return new QuestionGradeDetail
                {
                    QuestionId = question.Id,
                    QuestionType = question.Type.ToString(),
                    Score = isCorrect ? question.Points : 0,
                    MaxScore = question.Points,
                    Feedback = isCorrect ? "Correct!" : $"Incorrect. The correct answer is: {question.CorrectAnswer}",
                    IsPartialCredit = false,
                    Confidence = 1.0f
                };
            }

            if (question.Type == QuestionType.FillInTheBlank)
            {
                var isCorrect = string.Equals(
                    studentAnswer.Trim(),
                    question.CorrectAnswer.Trim(),
                    StringComparison.OrdinalIgnoreCase);

                float score = isCorrect ? question.Points : 0;
                string feedback = isCorrect ? "Correct!" : $"Incorrect. Expected: {question.CorrectAnswer}";

                return new QuestionGradeDetail
                {
                    QuestionId = question.Id,
                    QuestionType = question.Type.ToString(),
                    Score = score,
                    MaxScore = question.Points,
                    Feedback = feedback,
                    IsPartialCredit = false,
                    Confidence = 1.0f
                };
            }

            if (question.Type is QuestionType.Essay or QuestionType.ShortAnswer)
            {
                if (string.IsNullOrWhiteSpace(studentAnswer))
                {
                    return new QuestionGradeDetail
                    {
                        QuestionId = question.Id,
                        QuestionType = question.Type.ToString(),
                        Score = 0,
                        MaxScore = question.Points,
                        Feedback = "No answer provided.",
                        IsPartialCredit = false,
                        Confidence = 1.0f,
                        RequiresTeacherReview = false
                    };
                }

                var modelAnswer = question.ModelAnswer ?? question.CorrectAnswer;

                try
                {
                    var essayGrade = await _ollamaService.GradeEssayAsync(
                        contextChunks,
                        question.Text,
                        question.Points,
                        modelAnswer,
                        studentAnswer,
                        cancellationToken);

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

                    return new QuestionGradeDetail
                    {
                        QuestionId = question.Id,
                        QuestionType = question.Type.ToString(),
                        Score = finalScore,
                        MaxScore = question.Points,
                        Feedback = feedback,
                        IsPartialCredit = finalScore > 0 && finalScore < question.Points,
                        Confidence = essayGrade.Confidence,
                        RequiresTeacherReview = requiresTeacherReview
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "AI grading failed for question {QuestionId}. Marking for teacher review.",
                        question.Id);

                    return new QuestionGradeDetail
                    {
                        QuestionId = question.Id,
                        QuestionType = question.Type.ToString(),
                        Score = 0,
                        MaxScore = question.Points,
                        Feedback = "AI grading is currently unavailable. Teacher review is required.",
                        IsPartialCredit = false,
                        Confidence = 0f,
                        RequiresTeacherReview = true
                    };
                }
            }

            return new QuestionGradeDetail
            {
                QuestionId = question.Id,
                QuestionType = question.Type.ToString(),
                Score = 0,
                MaxScore = question.Points,
                Feedback = "Unknown question type.",
                IsPartialCredit = false,
                Confidence = 0,
                RequiresTeacherReview = true
            };
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
