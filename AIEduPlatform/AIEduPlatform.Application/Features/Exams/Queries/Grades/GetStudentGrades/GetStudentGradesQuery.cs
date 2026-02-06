using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetStudentGrades
{
    public record GetStudentGradesQuery : IRequest<List<GradeDto>>
    {
    }
}
