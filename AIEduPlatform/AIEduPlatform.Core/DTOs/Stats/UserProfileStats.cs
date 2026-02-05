namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Statistics about a user's profile
    /// </summary>
    public class UserProfileStats
    {
        public int CoursesEnrolled { get; set; }
        public int CoursesCompleted { get; set; }
        public int CoursesTaught { get; set; }
        public int TotalStudySessions { get; set; }
        public int ExamsTaken { get; set; }
        public float AverageExamScore { get; set; }
        public int FlashcardsCreated { get; set; }
        public int QuizzesTaken { get; set; }
        public TimeSpan TotalStudyTime { get; set; }
        public DateTime LastActiveDate { get; set; }
    }
}
