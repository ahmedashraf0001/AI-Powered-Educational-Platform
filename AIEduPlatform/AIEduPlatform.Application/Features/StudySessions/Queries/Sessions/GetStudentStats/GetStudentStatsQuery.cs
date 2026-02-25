using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.StudySessions.Queries.Sessions.GetStudentStats
{
    public record GetStudentStatsQuery : IRequest<StudentSessionStats>
    {
        public Guid? CourseId { get; init; }
    }
}
