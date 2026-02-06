using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission
{
    public record GradeSubmissionCommand : IRequest<Guid>
    {
        public Guid SubmissionId { get; init; }
        public float Score { get; init; }
        public string Feedback { get; init; } = string.Empty;
    }
}
