using AIEduPlatform.Core.DTOs.AI.Simple;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sections.SummarizeSection
{
    public record SummarizeSectionCommand : IRequest<Summary>
    {
        public Guid SessionId { get; init; }
        public Guid SectionId { get; init; }
        public int SummaryLength { get; init; } = 500;
        public bool IncludeKeyPoints { get; init; } = true;
    }
}
