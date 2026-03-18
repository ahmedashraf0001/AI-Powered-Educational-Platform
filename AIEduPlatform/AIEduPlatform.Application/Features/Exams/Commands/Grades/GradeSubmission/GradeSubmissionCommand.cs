using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmission
{
    public record GradeSubmissionCommand : IRequest<Guid>
    {
        public Guid SubmissionId { get; init; }
        public string Feedback { get; init; } = string.Empty;

        /// <summary>
        /// Per-question grades for written questions only.
        /// Key: QuestionId, Value: Points awarded (0 to question.Points)
        /// Objective questions are auto-calculated.
        /// </summary>
        public Dictionary<Guid, float> QuestionGrades { get; init; } = new();
    }
}
