using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetAvailableExamsForStudent
{
    public class GetAvailableExamsForStudentQueryHandler : IRequestHandler<GetAvailableExamsForStudentQuery, List<ExamDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetAvailableExamsForStudentQueryHandler> _logger;

        public GetAvailableExamsForStudentQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetAvailableExamsForStudentQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<ExamDto>> Handle(GetAvailableExamsForStudentQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view available exams.");
            }

            _logger.LogInformation("Fetching available exams for student {StudentId}", userId.Value);

            var exams = await _unitOfWork.Exams.GetAvailableExamsForStudentAsync(userId.Value, cancellationToken);

            return exams.Select(e => new ExamDto
            {
                Id = e.Id,
                CourseId = e.CourseId,
                Title = e.Title,
                StartTime = e.StartTime,
                EndTime = e.EndTime,
                DurationMinutes = e.DurationMinutes,
                QuestionCount = e.Questions?.Count ?? 0
            }).ToList();
        }
    }
}
