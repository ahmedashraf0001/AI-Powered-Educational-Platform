using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public record GetEnrolledCoursesQuery : IRequest<List<EnrollmentDto>>;
}
