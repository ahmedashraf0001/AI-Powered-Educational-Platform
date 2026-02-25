using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetSessionById
{
    public record GetSessionByIdQuery : IRequest<SessionDetailDto>
    {
        public Guid SessionId { get; init; }
    }
}
