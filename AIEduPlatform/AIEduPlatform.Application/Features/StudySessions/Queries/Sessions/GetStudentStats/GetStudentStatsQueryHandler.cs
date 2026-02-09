using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentStats
{
    public class GetStudentStatsQueryHandler : IRequestHandler<GetStudentStatsQuery, StudentSessionStats>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetStudentStatsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<StudentSessionStats> Handle(GetStudentStatsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            return await _unitOfWork.StudySessions.GetStudentStatsAsync(
                userId.Value, request.CourseId, cancellationToken);
        }
    }
}
