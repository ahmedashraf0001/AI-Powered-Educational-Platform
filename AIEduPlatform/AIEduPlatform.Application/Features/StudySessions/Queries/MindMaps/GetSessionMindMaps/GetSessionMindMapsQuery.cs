using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps
{
    public record GetSessionMindMapsQuery : IRequest<List<MindMapDto>>
    {
        public Guid SessionId { get; init; }
    }
}
