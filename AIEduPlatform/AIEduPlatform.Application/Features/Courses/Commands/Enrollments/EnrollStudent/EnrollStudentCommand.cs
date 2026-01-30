using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent
{
    public record EnrollStudentCommand : IRequest<Guid>
    {
        public Guid CourseId { get; init; }
    }
}
