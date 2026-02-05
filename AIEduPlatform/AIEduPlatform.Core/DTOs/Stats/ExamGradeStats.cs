namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Statistics about grades for an exam
    /// </summary>
    public class ExamGradeStats
    {
        public int TotalGraded { get; set; }
        public int PendingApproval { get; set; }
        public float AverageScore { get; set; }
        public float MedianScore { get; set; }
        public float HighestScore { get; set; }
        public float LowestScore { get; set; }
        public float PassRate { get; set; }
    }
}
