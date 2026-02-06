using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetPendingApprovalGrades
{
    public class GetPendingApprovalGradesQueryHandler : IRequestHandler<GetPendingApprovalGradesQuery, List<GradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetPendingApprovalGradesQueryHandler> _logger;

        public GetPendingApprovalGradesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetPendingApprovalGradesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<GradeDto>> Handle(GetPendingApprovalGradesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view pending approval grades.");
            }

            _logger.LogInformation("Fetching pending approval grades. ExamId: {ExamId}", request.ExamId);

            var grades = await _unitOfWork.Grades.GetPendingApprovalGradesAsync(request.ExamId, cancellationToken);

            return grades.Select(g => new GradeDto
            {
                Id = g.Id,
                SubmissionId = g.SubmissionId,
                Score = g.Score,
                Feedback = g.Feedback,
                IsAiGraded = g.IsAiGraded,
                IsApproved = g.IsApproved
            }).ToList();
        }
    }
}
