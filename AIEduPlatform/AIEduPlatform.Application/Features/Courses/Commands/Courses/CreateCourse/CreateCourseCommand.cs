using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse
{
    public record CreateCourseCommand : IRequest<Guid>
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public List<Guid>? CategoryIds { get; init; }
        public Stream? ThumbnailStream { get; init; }
        public string? ThumbnailFileName { get; init; }
        public string? ThumbnailContentType { get; init; }
    }
}
