using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public record GetEnrolledCoursesQuery : IRequest<PagedResult<EnrollmentDto>>
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
