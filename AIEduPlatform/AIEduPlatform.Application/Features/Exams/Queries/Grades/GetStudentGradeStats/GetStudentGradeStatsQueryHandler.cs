using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGradeStats
{
    public class GetStudentGradeStatsQueryHandler : IRequestHandler<GetStudentGradeStatsQuery, StudentGradeStats>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentGradeStatsQueryHandler> _logger;

        public GetStudentGradeStatsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentGradeStatsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<StudentGradeStats> Handle(GetStudentGradeStatsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view grade stats.");

            // Students can only view their own stats
            if (userId.Value != request.StudentId)
                throw new ForbiddenException("You can only view your own grade statistics.");

            var stats = await _unitOfWork.Grades.GetStudentStatsAsync(
                request.StudentId, request.CourseId, cancellationToken);

            return stats;
        }
    }
}
