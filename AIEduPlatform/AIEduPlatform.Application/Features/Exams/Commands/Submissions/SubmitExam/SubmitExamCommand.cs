using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Submissions.SubmitExam
{
    public record SubmitExamCommand : IRequest<Guid>
    {
        public Guid ExamId { get; init; }
        public Dictionary<Guid, string> Answers { get; init; } = [];
    }
}
