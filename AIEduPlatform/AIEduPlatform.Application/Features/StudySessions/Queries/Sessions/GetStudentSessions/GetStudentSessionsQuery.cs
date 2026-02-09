using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions
{
    public record GetStudentSessionsQuery : IRequest<List<SessionSummaryDto>>
    {
        public Guid? CourseId { get; init; }
    }
}
