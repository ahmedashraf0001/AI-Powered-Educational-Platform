namespace AIEduPlatform.Core.DTOs.Courses.Requests
{
    public record CreateCourseRequest
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
