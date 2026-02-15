using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeDistribution
{
    public class GetGradeDistributionQueryHandler : IRequestHandler<GetGradeDistributionQuery, Dictionary<string, int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetGradeDistributionQueryHandler> _logger;

        public GetGradeDistributionQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetGradeDistributionQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Dictionary<string, int>> Handle(GetGradeDistributionQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to view grade distribution.");

            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);
            if (exam == null)
                throw new NotFoundException(nameof(Exam), request.ExamId);

            if (exam.Course.TeacherId != userId.Value)
                throw new ForbiddenException("Only the course instructor can view grade distribution.");

            var distribution = await _unitOfWork.Grades.GetGradeDistributionAsync(request.ExamId, cancellationToken);

            return distribution;
        }
    }
}
