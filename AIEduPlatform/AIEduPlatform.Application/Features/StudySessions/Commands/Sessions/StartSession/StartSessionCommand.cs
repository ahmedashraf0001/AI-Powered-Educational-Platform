using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.StartSession
{
    public record StartSessionCommand : IRequest<Guid>
    {
        public Guid CourseId { get; init; }
    }
}
