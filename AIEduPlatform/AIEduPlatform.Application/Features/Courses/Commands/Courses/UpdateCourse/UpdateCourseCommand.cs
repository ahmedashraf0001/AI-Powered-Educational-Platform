using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse
{
    public record UpdateCourseCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal? Price { get; init; }
        public Guid? CategoryId { get; init; }
        public Stream? ThumbnailStream { get; init; }
        public string? ThumbnailFileName { get; init; }
        public string? ThumbnailContentType { get; init; }
        public bool RemoveThumbnail { get; init; }
    }
}
