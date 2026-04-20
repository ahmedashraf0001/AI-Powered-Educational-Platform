using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse
{
    public enum CourseRemovalReason
    {
        InstructorRequest,
        PolicyViolation,
        LegalRequest
    }

    public record DeleteCourseResult
    {
        public bool PermanentlyDeleted { get; init; }
        public bool Unpublished { get; init; }
        public bool AccessRevoked { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public record DeleteCourseCommand : IRequest<DeleteCourseResult>
    {
        public Guid CourseId { get; init; }
        public CourseRemovalReason Reason { get; init; } = CourseRemovalReason.InstructorRequest;
    }
}
