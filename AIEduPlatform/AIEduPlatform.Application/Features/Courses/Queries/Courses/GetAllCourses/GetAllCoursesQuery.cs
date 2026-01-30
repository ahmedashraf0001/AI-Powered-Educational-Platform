using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses
{
    public record GetAllCoursesQuery : IRequest<List<CourseListDto>>
    {
        public bool OnlyPublished { get; init; } = true;
    }
}
