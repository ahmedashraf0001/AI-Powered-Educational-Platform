using AIEduPlatform.Core.DTOs.AI.Simple;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Summaries.GenerateSummary
{
    public record GenerateSummaryCommand : IRequest<Summary>
    {
        public Guid SessionId { get; init; }
        public string Topic { get; init; } = string.Empty;
        public int SummaryLength { get; init; } = 500;
        public bool IncludeKeyPoints { get; init; } = true;
        public Guid? LectureId { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }
}
