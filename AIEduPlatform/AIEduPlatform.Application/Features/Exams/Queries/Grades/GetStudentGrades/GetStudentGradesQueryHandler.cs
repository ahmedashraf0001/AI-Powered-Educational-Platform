using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGrades
{
    public class GetStudentGradesQueryHandler : IRequestHandler<GetStudentGradesQuery, PagedResult<GradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentGradesQueryHandler> _logger;

        public GetStudentGradesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentGradesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<GradeDto>> Handle(GetStudentGradesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view your grades.");

            var allGrades = await _unitOfWork.Grades.GetGradesByStudentIdAsync(
                userId.Value, true, cancellationToken);
            
            var totalCount = allGrades.Count;
            var grades = allGrades
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = grades.Select(g => new GradeDto
            {
                Id = g.Id,
                SubmissionId = g.SubmissionId,
                Score = g.Score,
                Feedback = g.Feedback,
                IsAiGraded = g.IsAiGraded,
                IsApproved = g.IsApproved,
                ExamId = g.Submission?.ExamId ?? Guid.Empty,
                ExamTitle = g.Submission?.Exam?.Title ?? "Unknown Exam",
                CourseTitle = g.Submission?.Exam?.Course?.Title ?? "Unknown Course",
                QuestionResults = DeserializeQuestionResults(g.QuestionResults)
            }).ToList();

            return new PagedResult<GradeDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
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
