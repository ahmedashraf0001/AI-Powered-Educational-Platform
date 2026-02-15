using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments
{
    public record GetCourseEnrollmentsQuery : IRequest<PagedResult<EnrollmentDto>>
    {
        public Guid CourseId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
