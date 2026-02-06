using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.UpdateGrade
{
    public record UpdateGradeCommand : IRequest<Unit>
    {
        public Guid GradeId { get; init; }
        public float Score { get; init; }
        public string Feedback { get; init; } = string.Empty;
    }
}
