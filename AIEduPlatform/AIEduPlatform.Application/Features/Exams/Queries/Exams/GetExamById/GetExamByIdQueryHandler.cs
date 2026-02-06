using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Exams.GetExamById
{
    public class GetExamByIdQueryHandler : IRequestHandler<GetExamByIdQuery, ExamDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamByIdQueryHandler> _logger;

        public GetExamByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<ExamDetailDto> Handle(GetExamByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view exam details.");
            }

            _logger.LogInformation("Fetching exam {ExamId} details", request.ExamId);

            var exam = await _unitOfWork.Exams.GetExamByIdAsync(
                request.ExamId,
                includeQuestions: request.IncludeQuestions,
                includeSubmissions: true,
                cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            return new ExamDetailDto
            {
                Id = exam.Id,
                CourseId = exam.CourseId,
                Title = exam.Title,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                DurationMinutes = exam.DurationMinutes,
                Questions = exam.Questions?.OrderBy(q => q.Order).Select(q => new QuestionDto
                {
                    Id = q.Id,
                    ExamId = q.ExamId,
                    Type = q.Type,
                    Text = q.Text,
                    Options = q.Options,
                    CorrectAnswer = q.CorrectAnswer,
                    Points = q.Points,
                    Order = q.Order
                }).ToList() ?? [],
                SubmissionCount = exam.Submissions?.Count ?? 0
            };
        }
    }
}
