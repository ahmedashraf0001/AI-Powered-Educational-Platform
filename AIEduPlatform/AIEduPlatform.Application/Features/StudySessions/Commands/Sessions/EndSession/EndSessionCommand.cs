using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.EndSession
{
    public record EndSessionCommand : IRequest<Unit>
    {
        public Guid SessionId { get; init; }
    }
}
