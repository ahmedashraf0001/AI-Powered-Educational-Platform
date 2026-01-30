using AIEduPlatform.Core.Domain.Enums;

namespace AIEduPlatform.Core.DTOs.Courses
{
    public record EnrollmentDto
    {
        public Guid Id { get; init; }
        public Guid StudentId { get; init; }
        public string StudentName { get; init; } = string.Empty;
        public Guid CourseId { get; init; }
        public string CourseTitle { get; init; } = string.Empty;
        public DateTime EnrolledAt { get; init; }
        public EnrollmentStatus Status { get; init; }
    }
}
