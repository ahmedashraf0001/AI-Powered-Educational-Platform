namespace AIEduPlatform.Core.DTOs.Stats
{
    /// <summary>
    /// Full engagement report for a course, sent to the teacher.
    /// </summary>
    public class CourseEngagementReport
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int TotalEnrolled { get; set; }
        public int ActiveStudents { get; set; }
        public int AtRiskStudents { get; set; }
        public double AverageEngagementScore { get; set; }
        public List<StudentEngagementDto> Students { get; set; } = [];
    }
}
