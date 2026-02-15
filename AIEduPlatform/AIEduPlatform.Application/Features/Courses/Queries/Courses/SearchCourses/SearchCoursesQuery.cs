using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses
{
    public record SearchCoursesQuery : IRequest<PagedResult<CourseListDto>>
    {
        public string Keyword { get; init; } = string.Empty;
        public bool OnlyPublished { get; init; } = true;
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 20;
    }
}
