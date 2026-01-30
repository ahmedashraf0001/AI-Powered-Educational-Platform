using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor
{
    public record GetCoursesByInstructorQuery : IRequest<List<CourseListDto>>
    {
        public Guid? InstructorId { get; init; }
        public bool IncludeUnpublished { get; init; } = true;
    }
}
