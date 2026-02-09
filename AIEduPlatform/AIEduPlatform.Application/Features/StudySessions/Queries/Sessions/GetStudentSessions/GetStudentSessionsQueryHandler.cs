using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions
{
    public class GetStudentSessionsQueryHandler : IRequestHandler<GetStudentSessionsQuery, List<SessionSummaryDto>>
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

        public async Task<List<SessionSummaryDto>> Handle(GetStudentSessionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var sessions = request.CourseId.HasValue
                ? await _unitOfWork.StudySessions.GetSessionsByStudentAndCourseAsync(
                    userId.Value, request.CourseId.Value, cancellationToken)
                : await _unitOfWork.StudySessions.GetSessionsByStudentIdAsync(
                    userId.Value, cancellationToken);

            return sessions.Select(s => new SessionSummaryDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                CourseName = s.Course?.Title ?? string.Empty,
                StartedAt = s.StartedAt,
                LastActivity = s.LastActivity
            }).ToList();
        }
    }
}
