using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.PublishCourse
{
    public record PublishCourseCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
        public bool IsPublished { get; init; }
    }
}
