namespace AIEduPlatform.Core.DTOs.Stats
{
    public class TeacherDashboardStats
    {
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int DraftCourses { get; set; }
        public int TotalEnrollments { get; set; }
        public int TotalStudents { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalLectures { get; set; }
        public double CompletionRate { get; set; }
        public int TotalExamsCreated { get; set; }
        public int PendingGradeApprovals { get; set; }
        public int UngradedSubmissions { get; set; }
        public List<RecentEnrollmentItem> RecentEnrollments { get; set; } = [];
        public List<CoursePerformanceItem> CoursePerformance { get; set; } = [];
        public List<EnrollmentTrendItem> EnrollmentTrend { get; set; } = [];
    }

    public class RecentEnrollmentItem
    {
        public string StudentName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public DateTime EnrolledAt { get; set; }
    }

    public class CoursePerformanceItem
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int EnrollmentCount { get; set; }
        public double AverageRating { get; set; }
        public double CompletionRate { get; set; }
        public decimal Revenue { get; set; }
    }

    public class EnrollmentTrendItem
    {
        public string Month { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
