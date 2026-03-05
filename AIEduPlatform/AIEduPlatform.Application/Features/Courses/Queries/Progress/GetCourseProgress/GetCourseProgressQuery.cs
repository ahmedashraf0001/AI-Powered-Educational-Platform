using AIEduPlatform.Core.DTOs.Progress;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Progress.GetCourseProgress
{
    public record GetCourseProgressQuery : IRequest<CourseProgressDto>
    {
        public Guid CourseId { get; init; }
    }
}
