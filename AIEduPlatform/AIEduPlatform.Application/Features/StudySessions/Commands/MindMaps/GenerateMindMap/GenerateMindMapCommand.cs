using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.MindMaps.GenerateMindMap
{
    public record GenerateMindMapCommand : IRequest<MindMapDto>
    {
        public Guid SessionId { get; init; }
        public string CentralTopic { get; init; } = string.Empty;
        public int MaxDepth { get; init; } = 3;
        public Guid? LectureId { get; init; }
        public List<Guid>? MaterialIds { get; init; }
    }
}
