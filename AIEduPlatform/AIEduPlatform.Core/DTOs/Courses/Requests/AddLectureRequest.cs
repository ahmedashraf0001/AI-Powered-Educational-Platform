namespace AIEduPlatform.Core.DTOs.Courses.Requests
{
    public record AddLectureRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
