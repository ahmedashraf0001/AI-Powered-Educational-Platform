using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.GenerateSectionFlashcards
{
    public record GenerateSectionFlashcardsCommand : IRequest<List<FlashcardDto>>
    {
        public Guid SessionId { get; init; }
        public Guid SectionId { get; init; }
        public int NumberOfCards { get; init; } = 10;
    }
}
