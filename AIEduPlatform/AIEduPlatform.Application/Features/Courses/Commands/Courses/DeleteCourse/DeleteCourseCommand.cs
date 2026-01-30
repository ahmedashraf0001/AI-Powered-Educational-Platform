using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse
{
    public record DeleteCourseCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
    }
}
