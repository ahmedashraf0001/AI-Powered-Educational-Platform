using System.Collections.Generic;

namespace AIEduPlatform.Core.DTOs.Exams
{
    public record GradeDto
    {
        public Guid Id { get; init; }
        public Guid SubmissionId { get; init; }
        public float Score { get; init; }
        public string Feedback { get; init; } = string.Empty;
        public bool IsAiGraded { get; init; }
        public bool IsApproved { get; init; }
        public string ExamTitle { get; init; } = string.Empty;
        public string CourseTitle { get; init; } = string.Empty;
        public Guid ExamId { get; init; }
        public List<QuestionResultDto> QuestionResults { get; init; } = new();
    }
}
