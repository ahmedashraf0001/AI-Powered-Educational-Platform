using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Flashcards.GetSessionFlashcards
{
    public record GetSessionFlashcardsQuery : IRequest<List<FlashcardDto>>
    {
        public Guid SessionId { get; init; }
    }
}
