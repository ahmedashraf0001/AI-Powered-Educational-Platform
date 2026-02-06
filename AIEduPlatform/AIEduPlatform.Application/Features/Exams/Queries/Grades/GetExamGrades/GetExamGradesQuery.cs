using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetExamGrades
{
    public record GetExamGradesQuery : IRequest<List<GradeDto>>
    {
        public Guid ExamId { get; init; }
    }
}
