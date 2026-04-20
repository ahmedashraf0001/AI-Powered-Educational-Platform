using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Reviews;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Reviews.Queries.GetCourseReviews
{
    public class GetCourseReviewsQueryHandler : IRequestHandler<GetCourseReviewsQuery, PagedResult<ReviewDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetCourseReviewsQueryHandler> _logger;

        public GetCourseReviewsQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetCourseReviewsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PagedResult<ReviewDto>> Handle(GetCourseReviewsQuery request, CancellationToken cancellationToken)
        {
            var courseExists = await _unitOfWork.Courses.CourseExistsAsync(request.CourseId, cancellationToken);
            if (!courseExists)
                throw new NotFoundException(nameof(Course), request.CourseId);

            var (reviews, totalCount) = await _unitOfWork.Reviews.GetPagedByCourseIdAsync(
                request.CourseId, request.Page, request.PageSize, cancellationToken);

            var pagedReviews = reviews
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    CourseId = r.CourseId,
                    StudentId = r.StudentId,
                    StudentName = r.Student?.FirstName != null && r.Student?.LastName != null ? $"{r.Student.FirstName} {r.Student.LastName}" : r.Student?.UserName ?? string.Empty,
                    StudentAvatarUrl = r.Student?.AvatarUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                }).ToList();

            _logger.LogInformation(
                "Retrieved {Count}/{Total} reviews for course {CourseId} (page {Page})",
                pagedReviews.Count, totalCount, request.CourseId, request.Page);

            return new PagedResult<ReviewDto>
            {
                Items = pagedReviews,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
