using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Application.Common.Services;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam
{
    public class SubmitExamCommandHandler : IRequestHandler<SubmitExamCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IAIGradingQueue _aiGradingQueue;
        private readonly ILogger<SubmitExamCommandHandler> _logger;

        public SubmitExamCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IAIGradingQueue aiGradingQueue,
            ILogger<SubmitExamCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _aiGradingQueue = aiGradingQueue;
            _logger = logger;
        }

        public async Task<Guid> Handle(SubmitExamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to submit an exam.");
            }

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for submission by user {UserId}.", request.ExamId, userId.Value);
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            var now = DateTime.UtcNow;
            if (now < exam.StartTime)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} before start time.", userId.Value, request.ExamId);
                throw new BadRequestException("The exam has not started yet.");
            }

            if (now > exam.EndTime)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} after end time.", userId.Value, request.ExamId);
                throw new BadRequestException("The exam has ended and is no longer accepting submissions.");
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(exam.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam {ExamId}.", exam.CourseId, request.ExamId);
                throw new NotFoundException(nameof(Course), exam.CourseId);
            }

            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(userId.Value, exam.CourseId, cancellationToken);

            if (!isEnrolled)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} without being enrolled in course {CourseId}.", userId.Value, request.ExamId, exam.CourseId);
                throw new ForbiddenException("You must be enrolled in the course to submit this exam.");
            }

            var existingSubmission = await _unitOfWork.Submissions.GetSubmissionByExamAndStudentAsync(
                request.ExamId,
                userId.Value,
                false,
                cancellationToken);

            if (existingSubmission != null)
            {
                _logger.LogWarning("User {UserId} attempted to submit exam {ExamId} multiple times.", userId.Value, request.ExamId);
                throw new BadRequestException("You have already submitted this exam.");
            }

            _logger.LogInformation(
                "Submitting exam. ExamId: {ExamId}, StudentId: {StudentId}",
                request.ExamId,
                userId.Value);

            try
            {
                var submission = new Submission
                {
                    ExamId = request.ExamId,
                    StudentId = userId.Value,
                    Answers = JsonSerializer.Serialize(request.Answers),
                    SubmittedAt = DateTime.UtcNow
                };

                await _unitOfWork.Submissions.AddAsync(submission, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Mark exam attempt as submitted
                await _unitOfWork.ExamAttempts.MarkAsSubmittedAsync(request.ExamId, userId.Value, cancellationToken);

                _logger.LogInformation(
                    "Exam submitted successfully. SubmissionId: {SubmissionId}, ExamId: {ExamId}, StudentId: {StudentId}",
                    submission.Id,
                    request.ExamId,
                    userId.Value);

                // --- Auto-grading logic ---
                var questions = await _unitOfWork.Questions.GetQuestionsByExamIdAsync(request.ExamId, cancellationToken);

                if (questions != null && questions.Count > 0)
                {
                    var objectiveTypes = new HashSet<QuestionType>
                    {
                        QuestionType.MultipleChoice,
                        QuestionType.TrueFalse,
                        QuestionType.FillInTheBlank
                    };

                    var objectiveQuestions = questions.Where(q => objectiveTypes.Contains(q.Type)).ToList();
                    bool hasWrittenQuestions = questions.Any(q =>
                        q.Type == QuestionType.Essay || q.Type == QuestionType.ShortAnswer);

                    int totalExamPoints = questions.Sum(q => q.Points);

                    // Always auto-grade objective questions first
                    float objectiveScore = 0;
                    var questionResults = new List<QuestionResultDto>();

                    foreach (var question in questions.OrderBy(q => q.Order))
                    {
                        var studentAnswer = request.Answers.GetValueOrDefault(question.Id, string.Empty);

                        if (objectiveTypes.Contains(question.Type))
                        {
                            bool isCorrect = string.Equals(
                                studentAnswer?.Trim() ?? "",
                                question.CorrectAnswer?.Trim() ?? "",
                                StringComparison.OrdinalIgnoreCase);

                            if (isCorrect)
                            {
                                objectiveScore += question.Points;
                            }

                            questionResults.Add(new QuestionResultDto
                            {
                                QuestionId = question.Id,
                                QuestionType = question.Type.ToString(),
                                Score = isCorrect ? question.Points : 0,
                                MaxScore = question.Points,
                                Feedback = isCorrect ? "Correct!" : $"Incorrect. The correct answer is: {question.CorrectAnswer}",
                                IsPartialCredit = false,
                                Confidence = 1.0f,
                                RequiresTeacherReview = false
                            });
                        }
                        else
                        {
                            // Written questions get 0 for now; AI grading will update them
                            questionResults.Add(new QuestionResultDto
                            {
                                QuestionId = question.Id,
                                QuestionType = question.Type.ToString(),
                                Score = 0,
                                MaxScore = question.Points,
                                Feedback = "Awaiting AI grading.",
                                IsPartialCredit = false,
                                Confidence = 0f,
                                RequiresTeacherReview = true
                            });
                        }
                    }

                    float initialPercentage = totalExamPoints > 0 ? (objectiveScore / totalExamPoints) * 100 : 0;

                    var grade = new Grade
                    {
                        SubmissionId = submission.Id,
                        Score = initialPercentage,
                        Feedback = hasWrittenQuestions
                            ? "Auto-graded (objective questions). Written questions pending AI review."
                            : "Auto-graded. All questions were objective.",
                        IsAiGraded = hasWrittenQuestions ? false : true,
                        IsApproved = hasWrittenQuestions ? false : true,
                        QuestionResults = JsonSerializer.Serialize(questionResults)
                    };

                    await _unitOfWork.Grades.AddAsync(grade, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (hasWrittenQuestions)
                    {
                        _logger.LogInformation(
                            "Exam has written questions. SubmissionId: {SubmissionId}. Enqueueing for AI grading.",
                            submission.Id);

                        await _aiGradingQueue.EnqueueAsync(
                            new AIGradingRequest(submission.Id, course.TeacherId),
                            cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation(
                            "Exam fully auto-graded. SubmissionId: {SubmissionId}, Score: {Score}",
                            submission.Id,
                            grade.Score);
                    }
                }

                // Notify teacher about the submission
                var student = await _unitOfWork.Users.GetUserByIdAsync(userId.Value, ct: cancellationToken);
                await _notificationService.NotifyExamSubmittedAsync(
                    course.TeacherId,
                    student?.FirstName ?? "A student",
                    exam.Title,
                    course.Title,
                    cancellationToken);

                return submission.Id;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException or BadRequestException))
            {
                _logger.LogError(
                    ex,
                    "Error submitting exam. ExamId: {ExamId}, StudentId: {StudentId}",
                    request.ExamId,
                    userId.Value);
                throw;
            }
        }
    }
}
