using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments
{
    public record GetCourseEnrollmentsQuery : IRequest<List<EnrollmentDto>>
    {
        public Guid CourseId { get; init; }
    }
}
