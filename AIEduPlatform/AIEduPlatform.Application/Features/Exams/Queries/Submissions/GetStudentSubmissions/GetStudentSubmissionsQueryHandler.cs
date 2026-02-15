using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions
{
    public class GetStudentSubmissionsQueryHandler : IRequestHandler<GetStudentSubmissionsQuery, PagedResult<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentSubmissionsQueryHandler> _logger;

        public GetStudentSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<SubmissionDto>> Handle(GetStudentSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view your submissions.");

            var (submissions, totalCount) = await _unitOfWork.Submissions.GetPagedAsync(
                s => s.StudentId == userId.Value,
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = submissions.Select(s => new SubmissionDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                StudentId = s.StudentId,
                SubmittedAt = s.SubmittedAt,
                IsGraded = s.Grade != null
            }).ToList();

            return new PagedResult<SubmissionDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
