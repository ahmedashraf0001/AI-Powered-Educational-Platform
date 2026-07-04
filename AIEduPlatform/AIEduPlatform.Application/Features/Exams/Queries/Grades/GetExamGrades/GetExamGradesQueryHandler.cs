using System.Text.Json;
using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades
{
    public class GetExamGradesQueryHandler : IRequestHandler<GetExamGradesQuery, PagedResult<GradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamGradesQueryHandler> _logger;

        public GetExamGradesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamGradesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<GradeDto>> Handle(GetExamGradesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view grades.");

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);
            if (exam == null)
                throw new NotFoundException(nameof(Exam), request.ExamId);

            var (grades, totalCount) = await _unitOfWork.Grades.GetPagedAsync(
                g => g.Submission.ExamId == request.ExamId,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

            var items = grades.Select(g => new GradeDto
            {
                Id = g.Id,
                SubmissionId = g.SubmissionId,
                Score = g.Score,
                Feedback = g.Feedback,
                IsAiGraded = g.IsAiGraded,
                IsApproved = g.IsApproved,
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
