using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Stats;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGradeStats
{
    public class GetExamGradeStatsQueryHandler : IRequestHandler<GetExamGradeStatsQuery, ExamGradeStats>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamGradeStatsQueryHandler> _logger;

        public GetExamGradeStatsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamGradeStatsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ExamGradeStats> Handle(GetExamGradeStatsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view exam grade stats.");

            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);
            if (exam == null)
                throw new NotFoundException(nameof(Exam), request.ExamId);

            if (exam.Course.TeacherId != userId.Value)
                throw new ForbiddenException("Only the course instructor can view exam grade statistics.");

            var stats = await _unitOfWork.Grades.GetExamStatsAsync(request.ExamId, cancellationToken);

            return stats;
        }
    }
}
