using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetSubmissionById
{
    public class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, SubmissionDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetSubmissionByIdQueryHandler> _logger;

        public GetSubmissionByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetSubmissionByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<SubmissionDetailDto> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view submission details.");
            }

            _logger.LogInformation("Fetching submission {SubmissionId}", request.SubmissionId);

            var submission = await _unitOfWork.Submissions.GetSubmissionByIdAsync(
                request.SubmissionId,
                includeExam: true,
                includeGrade: true,
                cancellationToken);

            if (submission == null)
            {
                throw new NotFoundException(nameof(Submission), request.SubmissionId);
            }

            return new SubmissionDetailDto
            {
                Id = submission.Id,
                ExamId = submission.ExamId,
                StudentId = submission.StudentId,
                Answers = submission.Answers,
                SubmittedAt = submission.SubmittedAt,
                Grade = submission.Grade != null ? new GradeDto
                {
                    Id = submission.Grade.Id,
                    SubmissionId = submission.Grade.SubmissionId,
                    Score = submission.Grade.Score,
                    Feedback = submission.Grade.Feedback,
                    IsAiGraded = submission.Grade.IsAiGraded,
                    IsApproved = submission.Grade.IsApproved
                } : null
            };
        }
    }
}
