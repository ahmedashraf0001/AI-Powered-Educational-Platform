using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.DeleteExam
{
    public record DeleteExamCommand : IRequest<Unit>
    {
        public Guid ExamId { get; init; }
    }
}
