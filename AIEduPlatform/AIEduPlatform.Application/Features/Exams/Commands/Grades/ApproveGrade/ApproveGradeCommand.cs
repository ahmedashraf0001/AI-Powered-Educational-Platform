using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.ApproveGrade
{
    public record ApproveGradeCommand : IRequest<Unit>
    {
        public Guid GradeId { get; init; }
    }
}
