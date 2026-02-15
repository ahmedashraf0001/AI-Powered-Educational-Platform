using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGradeStats
{
    public record GetStudentGradeStatsQuery : IRequest<StudentGradeStats>
    {
        public Guid StudentId { get; init; }
        public Guid? CourseId { get; init; }
    }
}
