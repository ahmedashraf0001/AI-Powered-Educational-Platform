using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions
{
    public class GetStudentSessionsQueryHandler : IRequestHandler<GetStudentSessionsQuery, PagedResult<SessionSummaryDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStudentSessionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<SessionSummaryDto>> Handle(GetStudentSessionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var (sessions, totalCount) = await _unitOfWork.StudySessions.GetPagedAsync(
                s => s.StudentId == userId.Value && (!request.CourseId.HasValue || s.CourseId == request.CourseId.Value),
                request.Page,
                request.PageSize,
                cancellationToken);

            var items = sessions.Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                CourseName = s.Course?.Title ?? string.Empty,
                StartedAt = s.StartedAt,
                LastActivity = s.LastActivity
            }).ToList();

            return new PagedResult<SessionSummaryDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
