using AIEduPlatform.Core.DTOs.Stats;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.GetCourseEngagement
{
    public record GetCourseEngagementQuery(Guid CourseId) : IRequest<CourseEngagementReport>;
}
