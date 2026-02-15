using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses
{
    public record GetAllCoursesQuery : IRequest<PagedResult<CourseListDto>>
    {
        public bool OnlyPublished { get; init; } = true;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
