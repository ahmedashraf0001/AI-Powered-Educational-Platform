using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendedCourses
{
    public class GetRecommendedCoursesQueryHandler : IRequestHandler<GetRecommendedCoursesQuery, List<CourseListDto>>
    {
        private const int DefaultTop = 10;
        private const int MaxTop = 30;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRecommendationService _recommendationService;
        private readonly ILogger<GetRecommendedCoursesQueryHandler> _logger;

        public GetRecommendedCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IRecommendationService recommendationService,
            ILogger<GetRecommendedCoursesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _recommendationService = recommendationService;
            _logger = logger;
        }

        public async Task<List<CourseListDto>> Handle(GetRecommendedCoursesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to get recommendations.");

            var top = request.Top <= 0
                ? DefaultTop
                : Math.Min(request.Top, MaxTop);

            _logger.LogInformation("Getting recommended courses. UserId: {UserId}, Top: {Top}", userId, top);

            var recommendedIds = await _recommendationService.GetRecommendedCoursesAsync(userId, top, cancellationToken);

            if (recommendedIds.Count == 0)
            {
                _logger.LogInformation("No recommendations found for user {UserId}", userId);
                return new List<CourseListDto>();
            }

            var courses = await _unitOfWork.Courses.GetSelectedCoursesAsync(
                recommendedIds,
                cancellationToken,
                new CourseIncludeOptions
                {
                    IncludeTeacher = true,
                    IncludeLectures = true,
                    IncludeEnrollments = true,
                    IncludeReviews = true,
                    IncludeCategories = true
                });

            var enrolledCourseIds = (await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                    userId,
                    includeCourse: false,
                    cancellationToken))
                .Where(e => e.Status != EnrollmentStatus.Dropped)
                .Select(e => e.CourseId)
                .ToHashSet();

            var courseLookup = courses
                .Where(c => c.IsPublished)
                .ToDictionary(c => c.Id, c => c);

            var orderedResult = new List<CourseListDto>(recommendedIds.Count);

            foreach (var courseId in recommendedIds)
            {
                if (!courseLookup.TryGetValue(courseId, out var course))
                {
                    continue;
                }

                if (enrolledCourseIds.Contains(course.Id))
                {
                    continue;
                }

                var firstCategory = course.CourseCategories?.FirstOrDefault();

                orderedResult.Add(new CourseListDto
                {
                    CourseId = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    TeacherId = course.TeacherId,
                    TeacherName = course.Teacher?.UserName ?? string.Empty,
                    IsPublished = course.IsPublished,
                    LectureCount = course.Lectures?.Count ?? 0,
                    EnrollmentCount = course.Enrollments?.Count ?? 0,
                    CreatedAt = course.CreatedAt,
                    IsEnrolled = false,
                    AverageRating = course.Reviews != null && course.Reviews.Count > 0
                        ? Math.Round(course.Reviews.Average(r => r.Rating), 2)
                        : 0,
                    ReviewCount = course.Reviews?.Count ?? 0,
                    CategoryId = firstCategory?.CategoryId,
                    CategoryName = firstCategory?.Category?.Name,
                    Price = course.Price,
                    IsFree = course.Price == 0,
                    ThumbnailUrl = course.ThumbnailUrl
                });
            }

            _logger.LogInformation(
                "Returning {Count} recommended courses for user {UserId}",
                orderedResult.Count,
                userId);

            return orderedResult;
        }
    }
}
