namespace AIEduPlatform.Core.DTOs.Exams
{
    public class QuestionResultDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionType { get; set; } = string.Empty;
        public float Score { get; set; }
        public float MaxScore { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public bool IsPartialCredit { get; set; }
        public float Confidence { get; set; }
        public bool RequiresTeacherReview { get; set; }
    }
}
