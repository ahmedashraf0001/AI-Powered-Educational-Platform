using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetExamSubmissions
{
    public class GetExamSubmissionsQueryHandler : IRequestHandler<GetExamSubmissionsQuery, List<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamSubmissionsQueryHandler> _logger;

        public GetExamSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<SubmissionDto>> Handle(GetExamSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view submissions.");
            }

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            _logger.LogInformation("Fetching submissions for exam {ExamId}", request.ExamId);

            var submissions = await _unitOfWork.Submissions.GetSubmissionsByExamIdAsync(
                request.ExamId,
                includeGrades: true,
                cancellationToken);

            return submissions.Select(s => new SubmissionDto
            {
                Id = s.Id,
                ExamId = s.ExamId,
                StudentId = s.StudentId,
                SubmittedAt = s.SubmittedAt,
                IsGraded = s.Grade != null
            }).ToList();
        }
    }
}
