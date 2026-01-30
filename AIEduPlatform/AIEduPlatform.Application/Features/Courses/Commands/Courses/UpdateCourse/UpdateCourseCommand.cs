using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.UpdateCourse
{
    public record UpdateCourseCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
