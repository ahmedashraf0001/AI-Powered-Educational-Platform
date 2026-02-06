using AIEduPlatform.Core.DTOs.Exams;
using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Queries.Grades.GetPendingApprovalGrades
{
    public record GetPendingApprovalGradesQuery : IRequest<List<GradeDto>>
    {
        public Guid? ExamId { get; init; }
    }
}
