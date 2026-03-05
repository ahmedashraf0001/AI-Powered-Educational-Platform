namespace AIEduPlatform.Core.DTOs.Grades
{
    public record StudentDashboardDto
    {
        public List<CourseProgressSummary> CourseProgress { get; init; } = [];
        public EngagementSummary Engagement { get; init; } = new();
        public ExamSummary ExamStats { get; init; } = new();
        public List<GradeTrendPoint> GradeTrend { get; init; } = [];
        public List<SubmissionHistoryItem> SubmissionHistory { get; init; } = [];
    }

    public record CourseProgressSummary
    {
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public int CompletedLessons { get; init; }
        public int TotalLessons { get; init; }
        public double ProgressPercentage { get; init; }
    }

    public record EngagementSummary
    {
        public int LessonsViewed { get; init; }
        public double TotalStudyHours { get; init; }
        public int TotalStudySessions { get; init; }
    }

    public record ExamSummary
    {
        public int ExamsTaken { get; init; }
        public float AverageScore { get; init; }
        public float HighestScore { get; init; }
        public float LowestScore { get; init; }
    }

    public record GradeTrendPoint
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public float AverageScore { get; init; }
        public int ExamCount { get; init; }
    }

    public record SubmissionHistoryItem
    {
        public Guid SubmissionId { get; init; }
        public string CourseName { get; init; } = string.Empty;
        public string ExamTitle { get; init; } = string.Empty;
        public float? Score { get; init; }
        public DateTime SubmittedAt { get; init; }
    }
}
