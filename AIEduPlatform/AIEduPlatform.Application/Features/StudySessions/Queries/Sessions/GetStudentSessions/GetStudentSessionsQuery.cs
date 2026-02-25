using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.StudySessions;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentSessions
{
    public record GetStudentSessionsQuery : IRequest<PagedResult<SessionSummaryDto>>
    {
        public Guid? CourseId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
