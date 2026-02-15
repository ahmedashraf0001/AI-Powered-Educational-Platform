using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Reviews;
using MediatR;

namespace AIEduPlatform.Application.Features.Reviews.Queries.GetCourseReviews
{
    public record GetCourseReviewsQuery : IRequest<PagedResult<ReviewDto>>
    {
        public Guid CourseId { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
