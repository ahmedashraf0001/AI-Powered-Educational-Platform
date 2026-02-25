using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Flashcards.GetSessionFlashcards
{
    public class GetSessionFlashcardsQueryHandler : IRequestHandler<GetSessionFlashcardsQuery, PagedResult<FlashcardDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetSessionFlashcardsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<PagedResult<FlashcardDto>> Handle(GetSessionFlashcardsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var session = await _unitOfWork.StudySessions.GetByIdAsync(request.SessionId, cancellationToken);
            if (session is null)
                throw new NotFoundException(nameof(StudySession), request.SessionId);
            if (session.StudentId != userId.Value)
                throw new ForbiddenException("You can only view your own flashcards.");

            var (flashcards, totalCount) = await _unitOfWork.Flashcards.GetPagedAsync(
                f => f.SessionId == request.SessionId,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

            var items = flashcards.Select(f => new FlashcardDto
            {
                Id = f.Id,
                Topic = f.Topic,
                FrontText = f.FrontText,
                BackText = f.BackText,
                CreatedAt = f.CreatedAt
            }).ToList();

            return new PagedResult<FlashcardDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
