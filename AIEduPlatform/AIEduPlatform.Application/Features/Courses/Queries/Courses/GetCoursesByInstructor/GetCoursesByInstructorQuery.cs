using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor
{
    public record GetCoursesByInstructorQuery : IRequest<PagedResult<CourseListDto>>
    {
        public Guid? InstructorId { get; init; }
        public bool IncludeUnpublished { get; init; } = true;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
