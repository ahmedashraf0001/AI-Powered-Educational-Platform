using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGrades
{
    public class GetStudentGradesQueryHandler : IRequestHandler<GetStudentGradesQuery, List<GradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetStudentGradesQueryHandler> _logger;

        public GetStudentGradesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetStudentGradesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<GradeDto>> Handle(GetStudentGradesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view your grades.");
            }

            _logger.LogInformation("Fetching grades for student {StudentId}", userId.Value);

            var grades = await _unitOfWork.Grades.GetGradesByStudentIdAsync(
                userId.Value,
                includeSubmission: true,
                cancellationToken);

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
