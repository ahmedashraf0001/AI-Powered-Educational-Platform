using AIEduPlatform.Core.DTOs.Courses;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCourseById
{
    public record GetCourseByIdQuery : IRequest<CourseDetailDto>
    {
        public Guid CourseId { get; init; }
    }
}
