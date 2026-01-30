using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.UpdateLecture
{
    public record UpdateLectureCommand : IRequest<Unit>
    {
        public Guid LectureId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int OrderIndex { get; init; }
    }
}
