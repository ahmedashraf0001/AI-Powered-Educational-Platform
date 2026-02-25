using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.CreateExam
{
    public record CreateExamCommand : IRequest<Guid>
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public DateTime StartTime { get; init; }
        public DateTime EndTime { get; init; }
        public int DurationMinutes { get; init; }
    }
}
