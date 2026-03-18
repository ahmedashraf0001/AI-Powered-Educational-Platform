using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission
{
    public class GradeSubmissionCommandHandler : IRequestHandler<GradeSubmissionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IAuditService _auditService;
        private readonly ILogger<GradeSubmissionCommandHandler> _logger;

        public GradeSubmissionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IAuditService auditService,
            ILogger<GradeSubmissionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<Guid> Handle(GradeSubmissionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to grade a submission.");
            }

            // Single query to get submission with exam and course - reduces 3-4 DB hits to 1
            var submission = await _unitOfWork.Submissions.GetSubmissionWithExamAndCourseAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission == null)
            {
                _logger.LogWarning("Submission {SubmissionId} not found for grading by user {UserId}.", request.SubmissionId, userId.Value);
                throw new NotFoundException(nameof(Submission), request.SubmissionId);
            }

            if (submission.Grade != null)
            {
                _logger.LogInformation("Submission {SubmissionId} already has a grade. It will be updated with manual grading.", request.SubmissionId);
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
                _logger.LogWarning("User {UserId} attempted to grade submission {SubmissionId} without permission.", userId.Value, request.SubmissionId);
                throw new ForbiddenException("You are not authorized to grade this submission.");
            }

            // Get questions for per-question grading
            var questions = await _unitOfWork.Questions.GetQuestionsByExamIdAsync(exam.Id, cancellationToken);
            if (questions == null || !questions.Any())
            {
                throw new BadRequestException("No questions found for this exam.");
            }

            // Parse student answers
            Dictionary<string, string> studentAnswers;
            try
            {
                studentAnswers = JsonSerializer.Deserialize<Dictionary<string, string>>(submission.Answers)
                    ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                studentAnswers = new Dictionary<string, string>();
            }

            // Calculate score based on question types
            float totalScore = 0;
            float maxScore = questions.Sum(q => q.Points);

            var objectiveTypes = new HashSet<QuestionType>
            {
                QuestionType.MultipleChoice,
                QuestionType.TrueFalse,
                QuestionType.FillInTheBlank
            };

            foreach (var question in questions)
            {
                if (objectiveTypes.Contains(question.Type))
                {
                    // Auto-calculate objective questions
                    var answer = studentAnswers.GetValueOrDefault(question.Id.ToString(), "");
                    bool isCorrect = question.Type == QuestionType.FillInTheBlank
                        ? string.Equals(answer.Trim(), question.CorrectAnswer?.Trim(), StringComparison.OrdinalIgnoreCase)
                        : string.Equals(answer.Trim(), question.CorrectAnswer?.Trim(), StringComparison.Ordinal);

                    if (isCorrect)
                    {
                        totalScore += question.Points;
                    }
                }
                else
                {
                    // Use teacher's per-question grade for written questions
                    if (request.QuestionGrades.TryGetValue(question.Id, out var points))
                    {
                        // Clamp to valid range
                        totalScore += Math.Clamp(points, 0, question.Points);
                    }
                }
            }

            var percentage = maxScore > 0 ? (totalScore / maxScore) * 100 : 0;

            _logger.LogInformation(
                "Grading submission. SubmissionId: {SubmissionId}, Score: {Score}/{MaxScore} ({Percentage}%), TeacherId: {TeacherId}",
                request.SubmissionId,
                totalScore,
                maxScore,
                percentage,
                userId.Value);

            try
            {
                Grade grade;
                if (submission.Grade != null)
                {
                    // Update existing grade
                    submission.Grade.Score = percentage;
                    submission.Grade.Feedback = request.Feedback;
                    submission.Grade.IsAiGraded = false;
                    submission.Grade.IsApproved = true;
                    grade = submission.Grade;
                }
                else
                {
                    grade = new Grade
                    {
                        SubmissionId = request.SubmissionId,
                        Score = percentage,
                        Feedback = request.Feedback,
                        IsAiGraded = false,
                        IsApproved = true
                    };
                    await _unitOfWork.Grades.AddAsync(grade, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Submission graded successfully. GradeId: {GradeId}, SubmissionId: {SubmissionId}, Score: {Score}%",
                    grade.Id,
                    request.SubmissionId,
                    percentage);

                await _notificationService.NotifySubmissionGradedAsync(
                    submission.StudentId, course.Title, exam.Title, (decimal)percentage, cancellationToken);

                await _auditService.LogGradeActionAsync(
                    userId.Value, "ManualGrade", request.SubmissionId, grade.Id,
                    $"Score: {percentage:F1}% ({totalScore}/{maxScore})", cancellationToken);

                return grade.Id;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error grading submission. SubmissionId: {SubmissionId}, TeacherId: {TeacherId}",
                    request.SubmissionId,
                    userId.Value);
                throw;
            }
        }
    }
}
