using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Flashcards.GenerateFlashcards
{
    public record GenerateFlashcardsCommand : IRequest<List<FlashcardDto>>
    {
        public Guid SessionId { get; init; }
        public string Topic { get; init; } = string.Empty;
        public int NumberOfCards { get; init; } = 10;
        public Guid? LectureId { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }
}
