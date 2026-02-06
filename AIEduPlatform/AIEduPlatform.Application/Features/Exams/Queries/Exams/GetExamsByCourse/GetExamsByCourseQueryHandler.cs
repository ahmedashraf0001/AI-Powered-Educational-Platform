using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamsByCourse
{
    public class GetExamsByCourseQueryHandler : IRequestHandler<GetExamsByCourseQuery, List<ExamDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamsByCourseQueryHandler> _logger;

        public GetExamsByCourseQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamsByCourseQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<ExamDto>> Handle(GetExamsByCourseQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view exams.");
            }

            _logger.LogInformation("Fetching exams for course {CourseId}", request.CourseId);

            var exams = await _unitOfWork.Exams.GetExamsByCourseIdAsync(request.CourseId, includeQuestions: true, cancellationToken);

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
