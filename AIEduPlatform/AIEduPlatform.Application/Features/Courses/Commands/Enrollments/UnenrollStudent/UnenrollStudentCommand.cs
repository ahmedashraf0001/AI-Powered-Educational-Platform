using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent
{
    public record UnenrollStudentCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
    }
}
