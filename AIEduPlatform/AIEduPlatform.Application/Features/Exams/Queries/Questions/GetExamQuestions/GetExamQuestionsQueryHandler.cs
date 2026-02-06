using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Exams;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Queries.Questions.GetExamQuestions
{
    public class GetExamQuestionsQueryHandler : IRequestHandler<GetExamQuestionsQuery, List<QuestionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetExamQuestionsQueryHandler> _logger;

        public GetExamQuestionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetExamQuestionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<QuestionDto>> Handle(GetExamQuestionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view questions.");
            }

            var exam = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);

            if (exam == null)
            {
                throw new NotFoundException(nameof(Exam), request.ExamId);
            }

            _logger.LogInformation("Fetching questions for exam {ExamId}", request.ExamId);

            var questions = await _unitOfWork.Questions.GetQuestionsByExamIdAsync(request.ExamId, cancellationToken);

            return questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                ExamId = q.ExamId,
                Type = q.Type,
                Text = q.Text,
                Options = q.Options,
                CorrectAnswer = q.CorrectAnswer,
                Points = q.Points,
                Order = q.Order
            }).ToList();
        }
    }
}
