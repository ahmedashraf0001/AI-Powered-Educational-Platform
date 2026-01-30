namespace AIEduPlatform.Core.DTOs.Courses.Requests
{
    public record UpdateLectureRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
