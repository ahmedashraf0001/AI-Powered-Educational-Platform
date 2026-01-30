using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.CreateCourse
{
    public record CreateCourseCommand : IRequest<Guid>
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
