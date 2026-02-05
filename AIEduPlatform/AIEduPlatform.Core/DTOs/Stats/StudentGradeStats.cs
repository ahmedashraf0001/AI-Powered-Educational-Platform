namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Statistics about a student's grades
    /// </summary>
    public class StudentGradeStats
    {
        public int TotalExamsTaken { get; set; }
        public float AverageScore { get; set; }
        public float HighestScore { get; set; }
        public float LowestScore { get; set; }
        public int TotalPointsEarned { get; set; }
        public int TotalPointsPossible { get; set; }
        public float OverallPercentage { get; set; }
    }
}
