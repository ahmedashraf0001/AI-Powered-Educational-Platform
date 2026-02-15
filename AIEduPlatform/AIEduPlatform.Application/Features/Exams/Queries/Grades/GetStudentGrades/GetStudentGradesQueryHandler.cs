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

            var (grades, totalCount) = await _unitOfWork.Grades.GetPagedAsync(
                g => g.Submission.StudentId == userId.Value,
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = grades.Select(g => new GradeDto
            {
                Id = g.Id,
                SubmissionId = g.SubmissionId,
                Score = g.Score,
                Feedback = g.Feedback,
                IsAiGraded = g.IsAiGraded,
                IsApproved = g.IsApproved
            }).ToList();

            return new PagedResult<GradeDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
