using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGradeStats
{
    public record GetExamGradeStatsQuery : IRequest<ExamGradeStats>
    {
        public Guid ExamId { get; init; }
    }
}
