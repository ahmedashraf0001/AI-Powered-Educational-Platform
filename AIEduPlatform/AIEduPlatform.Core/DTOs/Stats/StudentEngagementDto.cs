namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Per-student engagement breakdown within a single course.
    /// </summary>
    public class StudentEngagementDto
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
        public string EnrollmentStatus { get; set; } = string.Empty;

        // ── Study session metrics ──
        public int TotalStudySessions { get; set; }
        public double TotalStudyHours { get; set; }
        public DateTime? LastStudySessionDate { get; set; }
        public int DaysSinceLastActivity { get; set; }

        // ── Chat / AI interaction metrics ──
        public int TotalChatMessages { get; set; }
        public int TotalFlashcardsGenerated { get; set; }
        public int TotalQuizzesTaken { get; set; }
        public int TotalMindMapsGenerated { get; set; }

        // ── Exam / grade metrics ──
        public int ExamsTaken { get; set; }
        public int ExamsAvailable { get; set; }
        public float AverageExamScore { get; set; }
        public int PendingSubmissions { get; set; }

        // ── Engagement score (0-100) ──
        public int EngagementScore { get; set; }
        public EngagementLevel EngagementLevel { get; set; }
    }

    public enum EngagementLevel
    {
        /// <summary>Score 0-25 — barely any activity</summary>
        Critical = 0,

        /// <summary>Score 26-50 — below average participation</summary>
        Low = 1,

        /// <summary>Score 51-75 — reasonable participation</summary>
        Moderate = 2,

        /// <summary>Score 76-100 — active and engaged</summary>
        High = 3
    }
}
