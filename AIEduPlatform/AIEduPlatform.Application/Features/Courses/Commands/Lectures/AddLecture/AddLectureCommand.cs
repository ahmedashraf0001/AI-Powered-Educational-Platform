using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.AddLecture
{
    public record AddLectureCommand : IRequest<Guid>
    {
        public Guid CourseId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
