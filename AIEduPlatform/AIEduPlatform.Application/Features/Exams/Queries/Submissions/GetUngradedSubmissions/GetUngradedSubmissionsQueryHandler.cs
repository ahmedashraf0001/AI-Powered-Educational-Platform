using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetUngradedSubmissions
{
    public class GetUngradedSubmissionsQueryHandler : IRequestHandler<GetUngradedSubmissionsQuery, List<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetUngradedSubmissionsQueryHandler> _logger;

        public GetUngradedSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetUngradedSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<SubmissionDto>> Handle(GetUngradedSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view ungraded submissions.");
            }

            _logger.LogInformation("Fetching ungraded submissions. ExamId: {ExamId}", request.ExamId);

            var submissions = await _unitOfWork.Submissions.GetUngradedSubmissionsAsync(request.ExamId, cancellationToken);

            return submissions.Select(s => new SubmissionDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                StudentId = s.StudentId,
                SubmittedAt = s.SubmittedAt,
                IsGraded = false
            }).ToList();
        }
    }
}
