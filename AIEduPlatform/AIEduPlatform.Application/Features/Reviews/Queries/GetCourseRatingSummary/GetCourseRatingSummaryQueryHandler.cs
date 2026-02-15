using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Reviews;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Reviews.Queries.GetCourseRatingSummary
{
    public class GetCourseRatingSummaryQueryHandler : IRequestHandler<GetCourseRatingSummaryQuery, CourseRatingSummaryDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCourseRatingSummaryQueryHandler> _logger;

        public GetCourseRatingSummaryQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCourseRatingSummaryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CourseRatingSummaryDto> Handle(GetCourseRatingSummaryQuery request, CancellationToken cancellationToken)
        {
            var courseExists = await _unitOfWork.Courses.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
                throw new NotFoundException(nameof(Course), request.CourseId);

            var (averageRating, totalReviews, distribution) = await _unitOfWork.Reviews
                .GetCourseRatingSummaryAsync(request.CourseId, cancellationToken);

            _logger.LogInformation(
                "Rating summary for course {CourseId}: Avg={Average}, Total={Total}",
                request.CourseId, averageRating, totalReviews);

            return new CourseRatingSummaryDto
            {
                CourseId = request.CourseId,
                AverageRating = Math.Round(averageRating, 2),
                TotalReviews = totalReviews,
                RatingDistribution = distribution
            };
        }
    }
}
