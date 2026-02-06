using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades
{
    public class GetExamGradesQueryHandler : IRequestHandler<GetExamGradesQuery, List<GradeDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamGradesQueryHandler> _logger;

        public GetExamGradesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamGradesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<GradeDto>> Handle(GetExamGradesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view grades.");
            }

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            _logger.LogInformation("Fetching grades for exam {ExamId}", request.ExamId);

            var grades = await _unitOfWork.Grades.GetGradesByExamIdAsync(request.ExamId, cancellationToken);

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
