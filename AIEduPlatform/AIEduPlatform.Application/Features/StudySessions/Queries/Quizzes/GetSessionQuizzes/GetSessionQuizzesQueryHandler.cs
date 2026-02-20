using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Quizzes.GetSessionQuizzes
{
    public class GetSessionQuizzesQueryHandler : IRequestHandler<GetSessionQuizzesQuery, PagedResult<GeneratedQuizDto>>
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

        public async Task<PagedResult<GeneratedQuizDto>> Handle(GetSessionQuizzesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only view your own quizzes.");

            var (quizzes, totalCount) = await _unitOfWork.GeneratedQuizzes.GetPagedAsync(
                q => q.SessionId == request.SessionId,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

            var items = quizzes.Select(q => new GeneratedQuizDto
            {
                Id = q.Id,
                Topic = q.Topic,
                Difficulty = q.Difficulty,
                Questions = q.Questions,
                StudentAnswers = q.StudentAnswers,
                Score = q.Score,
                CreatedAt = q.CreatedAt
            }).ToList();

            return new PagedResult<GeneratedQuizDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
