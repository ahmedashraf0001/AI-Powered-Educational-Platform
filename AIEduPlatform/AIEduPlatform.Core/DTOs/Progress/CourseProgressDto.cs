namespace AIEduPlatform.Core.DTOs.Progress
{
    public record CourseProgressDto
    {
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public int CompletedLessons { get; init; }
        public int TotalLessons { get; init; }
        public double ProgressPercentage { get; init; }
        public bool IsCompleted { get; init; }
        public List<Guid> CompletedLectureIds { get; init; } = [];
        public List<Guid> CompletedMaterialIds { get; init; } = [];
    }

    public record ContinueLearningDto
    {
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public double ProgressPercentage { get; init; }
        public Guid? LastMaterialId { get; init; }
        public string? LastMaterialTitle { get; init; }
        public Guid? LectureId { get; init; }
        public int? ResumePosition { get; init; }
    }
}
