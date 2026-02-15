using AIEduPlatform.Core.DTOs.Reviews;
using MediatR;

namespace AIEduPlatform.Application.Features.Reviews.Queries.GetCourseRatingSummary
{
    public record GetCourseRatingSummaryQuery : IRequest<CourseRatingSummaryDto>
    {
        public Guid CourseId { get; init; }
    }
}
