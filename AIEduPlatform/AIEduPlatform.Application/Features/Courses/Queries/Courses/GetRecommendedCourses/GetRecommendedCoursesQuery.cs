using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendedCourses
{
    public record GetRecommendedCoursesQuery : IRequest<List<CourseListDto>>
    {
        public int Top { get; init; } = 10;
    }
}
