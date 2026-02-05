namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Statistics about a student's study sessions
    /// </summary>
    public class StudentSessionStats
    {
        public int TotalSessions { get; set; }
        public int TotalMessages { get; set; }
        public int TotalFlashcards { get; set; }
        public int TotalQuizzes { get; set; }
        public int TotalMindMaps { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public DateTime? LastSessionDate { get; set; }
    }
}
