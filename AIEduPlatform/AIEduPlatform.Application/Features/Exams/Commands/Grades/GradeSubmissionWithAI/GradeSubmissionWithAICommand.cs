using MediatR;

namespace AIEduPlatform.Application.Features.Exams.Commands.Grades.GradeSubmissionWithAI
{
    /// <summary>
    /// Command to grade a submission using AI with partial credit scoring for essay/short answer questions
    /// </summary>
    public record GradeSubmissionWithAICommand : IRequest<GradeSubmissionWithAIResult>
    {
        /// <summary>
        /// The submission to grade
        /// </summary>
        public Guid SubmissionId { get; init; }
    }

    /// <summary>
    /// Result of AI grading including detailed scoring breakdown
    /// </summary>
    public record GradeSubmissionWithAIResult
    {
        /// <summary>
        /// Whether grading was successful
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// ID of the created grade
        /// </summary>
        public Guid? GradeId { get; init; }

        /// <summary>
        /// Total score achieved
        /// </summary>
        public float TotalScore { get; init; }

        /// <summary>
        /// Maximum possible score
        /// </summary>
        public float MaxScore { get; init; }

        /// <summary>
        /// Percentage score
        /// </summary>
        public float Percentage { get; init; }

        /// <summary>
        /// Combined feedback for the student
        /// </summary>
        public string Feedback { get; init; } = string.Empty;

        /// <summary>
        /// Whether teacher review is recommended
        /// </summary>
        public bool RequiresTeacherReview { get; init; }

        /// <summary>
        /// Individual question grades with detailed breakdown
        /// </summary>
        public List<QuestionGradeDetail> QuestionGrades { get; init; } = new();

        /// <summary>
        /// Error message if grading failed
        /// </summary>
        public string? Error { get; init; }
    }

    /// <summary>
    /// Detailed grade for a single question
    /// </summary>
    public record QuestionGradeDetail
    {
        public Guid QuestionId { get; init; }
        public string QuestionType { get; init; } = string.Empty;
        public float Score { get; init; }
        public float MaxScore { get; init; }
        public string Feedback { get; init; } = string.Empty;
        public bool IsPartialCredit { get; init; }
        public float Confidence { get; init; }
        public bool RequiresTeacherReview { get; init; }
    }
}
