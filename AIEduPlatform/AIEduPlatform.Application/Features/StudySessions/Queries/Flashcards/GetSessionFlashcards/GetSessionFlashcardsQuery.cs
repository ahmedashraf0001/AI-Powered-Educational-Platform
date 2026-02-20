using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Flashcards.GetSessionFlashcards
{
    public record GetSessionFlashcardsQuery : IRequest<PagedResult<FlashcardDto>>
    {
        public Guid SessionId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 50;
    }
}
