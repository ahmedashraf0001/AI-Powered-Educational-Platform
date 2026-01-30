using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses
{
    public record SearchCoursesQuery : IRequest<List<CourseListDto>>
    {
        public string Keyword { get; init; } = string.Empty;
        public bool OnlyPublished { get; init; } = true;
    }
}
