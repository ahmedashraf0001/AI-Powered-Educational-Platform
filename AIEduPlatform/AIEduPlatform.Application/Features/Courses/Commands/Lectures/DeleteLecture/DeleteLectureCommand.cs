using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Lectures.DeleteLecture
{
    public record DeleteLectureCommand : IRequest<Unit>
    {
        public Guid LectureId { get; init; }
    }
}
