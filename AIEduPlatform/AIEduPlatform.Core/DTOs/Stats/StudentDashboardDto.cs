namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Comprehensive student academic performance dashboard.
    /// Designed to be frontend-friendly with minimal transformation needed.
    /// </summary>
    public class StudentDashboardDto
    {
        // ── Summary counts ──
        public int TotalEnrolledCourses { get; set; }
        public int CompletedCourses { get; set; }
        public int InProgressCourses { get; set; }
        public int TotalLecturesCompleted { get; set; }
        public int TotalLectures { get; set; }
        public double OverallProgressPercentage { get; set; }
        public int CertificatesEarned { get; set; }

        /// <summary>
        /// Overall course progress across all enrolled courses.
        /// </summary>
        public List<CourseProgressSummary> CourseProgress { get; set; } = new();

        /// <summary>
        /// Engagement analytics — sessions, time spent, materials viewed.
        /// </summary>
        public EngagementAnalytics Engagement { get; set; } = new();

        /// <summary>
        /// Academic performance statistics.
        /// </summary>
        public AcademicPerformance Performance { get; set; } = new();

        /// <summary>
        /// Grade trend data grouped by month for chart rendering.
        /// </summary>
        public List<GradeTrendPoint> GradeTrend { get; set; } = new();

        /// <summary>
        /// Recent exam submission history.
        /// </summary>
        public List<SubmissionHistoryItem> SubmissionHistory { get; set; } = new();

        /// <summary>
        /// Recent activity feed.
        /// </summary>
        public List<RecentActivityItem> RecentActivity { get; set; } = new();
    }

    public class RecentActivityItem
    {
        public string CourseTitle { get; set; } = string.Empty;
        public string LectureTitle { get; set; } = string.Empty;
        public DateTime? CompletedAt { get; set; }
    }

    public class CourseProgressSummary
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int CompletedMaterials { get; set; }
        public int TotalMaterials { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime EnrolledAt { get; set; }
    }

    public class EngagementAnalytics
    {
        public int TotalStudySessions { get; set; }
        public int TotalMaterialsViewed { get; set; }
        public double TotalTimeSpentMinutes { get; set; }
        public int TotalQuizzesGenerated { get; set; }
        public int TotalFlashcardsGenerated { get; set; }
        public int CoursesEnrolled { get; set; }
        public int CoursesCompleted { get; set; }
    }

    public class AcademicPerformance
    {
        public int ExamsTaken { get; set; }
        public float AverageScore { get; set; }
        public float HighestScore { get; set; }
        public float LowestScore { get; set; }
    }

    public class GradeTrendPoint
    {
        public string Month { get; set; } = string.Empty; // "2024-01"
        public float AverageScore { get; set; }
        public int ExamCount { get; set; }
    }

    public class SubmissionHistoryItem
    {
        public Guid SubmissionId { get; set; }
        public string ExamTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public float? Score { get; set; }
        public DateTime SubmittedAt { get; set; }
        public bool IsGraded { get; set; }
    }
}
