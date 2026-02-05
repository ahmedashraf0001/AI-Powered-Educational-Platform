namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Statistics about exam submissions
    /// </summary>
    public class SubmissionStats
    {
        public int TotalSubmissions { get; set; }
        public int GradedCount { get; set; }
        public int PendingGradeCount { get; set; }
        public int AiGradedCount { get; set; }
        public int ApprovedCount { get; set; }
        public float? AverageScore { get; set; }
        public float? HighestScore { get; set; }
        public float? LowestScore { get; set; }
    }
}
