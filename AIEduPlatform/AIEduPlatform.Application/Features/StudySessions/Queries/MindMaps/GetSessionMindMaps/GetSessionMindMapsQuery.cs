using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.MindMaps.GetSessionMindMaps
{
    public record GetSessionMindMapsQuery : IRequest<PagedResult<MindMapDto>>
    {
        public Guid SessionId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
