using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetGradeBySubmission
{
    public class GetGradeBySubmissionQueryHandler : IRequestHandler<GetGradeBySubmissionQuery, GradeDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetGradeBySubmissionQueryHandler> _logger;

        public GetGradeBySubmissionQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetGradeBySubmissionQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<GradeDto> Handle(GetGradeBySubmissionQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view grade details.");
            }

            _logger.LogInformation("Fetching grade for submission {SubmissionId}", request.SubmissionId);

            var grade = await _unitOfWork.Grades.GetGradeBySubmissionIdAsync(request.SubmissionId, cancellationToken);

            if (grade == null)
            {
                throw new NotFoundException(nameof(Grade), request.SubmissionId);
            }

            return new GradeDto
            {
                Id = grade.Id,
                SubmissionId = grade.SubmissionId,
                Score = grade.Score,
                Feedback = grade.Feedback,
                IsAiGraded = grade.IsAiGraded,
                IsApproved = grade.IsApproved
            };
        }
    }
}
