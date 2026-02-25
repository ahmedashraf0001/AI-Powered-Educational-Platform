using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetActiveExams
{
    public class GetActiveExamsQueryHandler : IRequestHandler<GetActiveExamsQuery, PagedResult<ExamDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetActiveExamsQueryHandler> _logger;

        public GetActiveExamsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetActiveExamsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<ExamDto>> Handle(GetActiveExamsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view exams.");

            var (exams, totalCount) = await _unitOfWork.Exams.GetActiveExamsPagedAsync(
                request.CourseId, request.Page, request.PageSize, cancellationToken);

            var items = exams.Select(e => new ExamDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                DurationMinutes = e.DurationMinutes,
                QuestionCount = e.Questions?.Count ?? 0
            }).ToList();

            return new PagedResult<ExamDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
