using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.CompleteEnrollment
{
    public record CompleteEnrollmentCommand : IRequest<Unit>
    {
        public Guid CourseId { get; init; }
    }
}
