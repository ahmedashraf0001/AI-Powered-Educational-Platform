namespace AIEduPlatform.Core.DTOs.Stats
{
    public class TeacherDashboardStats
    {
        public int TotalCourses { get; set; }
        public int PublishedCourses { get; set; }
        public int TotalStudentsEnrolled { get; set; }
        public int TotalExamsCreated { get; set; }
        public int PendingGradeApprovals { get; set; }
        public int UngradedSubmissions { get; set; }
    }
}
