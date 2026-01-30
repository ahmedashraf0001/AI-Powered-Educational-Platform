namespace AIEduPlatform.Core.DTOs.Courses.Requests
{
    public record UpdateCourseRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
