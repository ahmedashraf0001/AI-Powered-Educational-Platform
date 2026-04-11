using System.Text;
using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
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

                    if (gradeDetail.Confidence < 0.7f)
                    {
                        requiresReview = true;
                    }

                    if (!string.IsNullOrEmpty(gradeDetail.Feedback))
                    {
                        var formattedScore = gradeDetail.Score % 1 == 0 ? gradeDetail.Score.ToString("0") : gradeDetail.Score.ToString("0.##");
                        feedbackBuilder.AppendLine($"Q{question.Order} | Score: {formattedScore}/{question.Points} | {gradeDetail.Feedback}");
                    }
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
                        IsApproved = !requiresReview
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
                        Confidence = 1.0f
                    };
                }

                try
                {
                    var modelAnswer = question.ModelAnswer ?? question.CorrectAnswer;

                    var essayGrade = await _ollamaService.GradeEssayAsync(
                        contextChunks,
                        question.Text,
                        question.Points,
                        modelAnswer,
                        studentAnswer,
                        cancellationToken);

                    var scaledScore = (essayGrade.Score / essayGrade.MaxPoints) * question.Points;

                    return new QuestionGradeDetail
                    {
                        QuestionId = question.Id,
                        QuestionType = question.Type.ToString(),
                        Score = scaledScore,
                        MaxScore = question.Points,
                        Feedback = essayGrade.Feedback,
                        IsPartialCredit = scaledScore > 0 && scaledScore < question.Points,
                        Confidence = essayGrade.Confidence
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
                        Feedback = "AI grading failed. Please review manually.",
                        IsPartialCredit = false,
                        Confidence = 0
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
                Confidence = 0
            };
        }
    }
}
