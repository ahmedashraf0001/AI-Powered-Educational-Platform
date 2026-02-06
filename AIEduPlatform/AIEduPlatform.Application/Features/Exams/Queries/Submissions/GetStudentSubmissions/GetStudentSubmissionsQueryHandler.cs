using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Submissions.GetStudentSubmissions
{
    public class GetStudentSubmissionsQueryHandler : IRequestHandler<GetStudentSubmissionsQuery, List<SubmissionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentSubmissionsQueryHandler> _logger;

        public GetStudentSubmissionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentSubmissionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<SubmissionDto>> Handle(GetStudentSubmissionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view your submissions.");
            }

            _logger.LogInformation("Fetching submissions for student {StudentId}", userId.Value);

            var submissions = await _unitOfWork.Submissions.GetSubmissionsByStudentIdAsync(
                userId.Value,
                includeExam: true,
                includeGrade: true,
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
