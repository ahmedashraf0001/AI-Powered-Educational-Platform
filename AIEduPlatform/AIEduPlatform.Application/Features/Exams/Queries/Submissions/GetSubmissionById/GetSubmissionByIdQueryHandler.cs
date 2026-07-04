using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetSubmissionById
{
    public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, SubmissionDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetSubmissionByIdQueryHandler> _logger;

        public GetSubmissionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetSubmissionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<SubmissionDetailDto> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view submission details.");
            }

            _logger.LogInformation("Fetching submission {SubmissionId}", request.SubmissionId);

            var submission = await _unitOfWork.Submissions.GetSubmissionWithExamAndCourseAsync(
                request.SubmissionId,
                cancellationToken);

            if (submission == null)
            {
                throw new NotFoundException(nameof(Submission), request.SubmissionId);
            }

            // Parse student answers from JSON
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

            // Get questions for this exam
            var questions = await _unitOfWork.Questions.GetQuestionsByExamIdAsync(
                submission.ExamId, cancellationToken);

            // Determine if correct answers should be shown
            // Show correct answers only if:
            // 1. User is the course teacher, OR
            // 2. Submission is graded and approved
            var isTeacher = submission.Exam?.Course?.TeacherId == userId.Value;
            var isGradedAndApproved = submission.Grade?.IsApproved == true;
            var showCorrectAnswers = isTeacher || isGradedAndApproved;

            // Build structured answer list
            var answers = (questions ?? new List<Question>())
                .OrderBy(q => q.Order)
                .Select(q =>
                {
                    var answer = studentAnswers.GetValueOrDefault(q.Id.ToString(), string.Empty);
                    var options = new List<string>();
                    if (!string.IsNullOrEmpty(q.Options))
                    {
                        try { options = JsonSerializer.Deserialize<List<string>>(q.Options) ?? new(); }
                        catch { /* ignore parse errors */ }
                    }
                    return new SubmissionAnswerDto
                    {
                        QuestionId = q.Id,
                        QuestionText = q.Text,
                        QuestionType = q.Type,
                        Answer = answer,
                        // Only show correct answer if teacher or graded/approved
                        CorrectAnswer = showCorrectAnswers ? (q.CorrectAnswer ?? string.Empty) : string.Empty,
                        Options = options,
                        Points = q.Points,
                        Order = q.Order
                    };
                })
                .ToList();

            // Get student name
            var student = await _unitOfWork.Users.GetUserByIdAsync(submission.StudentId, ct: cancellationToken);

            return new SubmissionDetailDto
            {
                Id = submission.Id,
                ExamId = submission.ExamId,
                StudentId = submission.StudentId,
                ExamTitle = submission.Exam?.Title ?? "Unknown Exam",
                CourseName = submission.Exam?.Course?.Title ?? "Unknown Course",
                StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Unknown Student",
                Answers = answers,
                SubmittedAt = submission.SubmittedAt,
                Grade = submission.Grade != null ? new GradeDto
                {
                    Id = submission.Grade.Id,
                    SubmissionId = submission.Grade.SubmissionId,
                    Score = submission.Grade.Score,
                    Feedback = submission.Grade.Feedback,
                    IsAiGraded = submission.Grade.IsAiGraded,
                    IsApproved = submission.Grade.IsApproved,
                    QuestionResults = DeserializeQuestionResults(submission.Grade.QuestionResults)
                } : null
            };
        }

        private static List<QuestionResultDto> DeserializeQuestionResults(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<QuestionResultDto>();

            try
            {
                return JsonSerializer.Deserialize<List<QuestionResultDto>>(json)
                    ?? new List<QuestionResultDto>();
            }
            catch (JsonException)
            {
                return new List<QuestionResultDto>();
            }
        }
    }
}
