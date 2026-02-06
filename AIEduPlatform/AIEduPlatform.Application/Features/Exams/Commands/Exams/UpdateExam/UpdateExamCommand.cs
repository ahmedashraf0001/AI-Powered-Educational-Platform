using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.UpdateExam
{
    public record UpdateExamCommand : IRequest<Unit>
    {
        public Guid ExamId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
    }
}
