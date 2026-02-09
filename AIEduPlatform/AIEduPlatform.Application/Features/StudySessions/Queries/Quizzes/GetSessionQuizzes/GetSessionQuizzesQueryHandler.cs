using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes
{
    public class GetSessionQuizzesQueryHandler : IRequestHandler<GetSessionQuizzesQuery, List<GeneratedQuizDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSessionQuizzesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<List<GeneratedQuizDto>> Handle(GetSessionQuizzesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only view your own quizzes.");

            var quizzes = await _unitOfWork.GeneratedQuizzes
                .GetBySessionIdAsync(request.SessionId, cancellationToken);

            return quizzes.Select(q => new GeneratedQuizDto
            {
                Id = q.Id,
                Topic = q.Topic,
                Difficulty = q.Difficulty,
                Questions = q.Questions,
                StudentAnswers = q.StudentAnswers,
                Score = q.Score,
                CreatedAt = q.CreatedAt
            }).ToList();
        }
    }
}
