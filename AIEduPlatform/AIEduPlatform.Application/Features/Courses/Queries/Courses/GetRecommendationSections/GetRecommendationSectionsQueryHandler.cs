using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Application.Features.Courses.Queries.Progress.GetContinueLearning;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetRecommendationSections
{
    public class GetRecommendationSectionsQueryHandler : IRequestHandler<GetRecommendationSectionsQuery, RecommendationSectionsDto>
    {
        private const int DefaultTop = 10;
        private const int MaxTop = 30;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IRecommendationService _recommendationService;
        private readonly IMediator _mediator;
        private readonly ILogger<GetRecommendationSectionsQueryHandler> _logger;

        public GetRecommendationSectionsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IRecommendationService recommendationService,
            IMediator mediator,
            ILogger<GetRecommendationSectionsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _recommendationService = recommendationService;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<RecommendationSectionsDto> Handle(GetRecommendationSectionsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to get recommendations.");

            var top = request.Top <= 0
                ? DefaultTop
                : Math.Min(request.Top, MaxTop);

            _logger.LogInformation("Building recommendation sections. UserId: {UserId}, Top: {Top}", userId, top);

            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                userId,
                includeCourse: false,
                cancellationToken);

            var enrolledCourseIds = enrollments
                .Where(e => e.Status != EnrollmentStatus.Dropped)
                .Select(e => e.CourseId)
                .ToHashSet();

            var topPickCandidates = await _recommendationService.GetRecommendedCoursesAsync(
                userId,
                top * 3,
                cancellationToken);

            var topPicksForYou = await BuildOrderedCourseDtosAsync(
                topPickCandidates,
                enrolledCourseIds,
                top,
                cancellationToken);

            var continueLearning = await _mediator.Send(new GetContinueLearningQuery(), cancellationToken);
            continueLearning = continueLearning
                .OrderByDescending(c => c.ProgressPercentage)
                .Take(top)
                .ToList();

            var anchorCourse = await ResolveAnchorCourseAsync(enrollments, cancellationToken);

            var becauseYouLearned = new List<CourseListDto>();
            if (anchorCourse != null)
            {
                var becauseCandidateIds = new List<Guid>();
                var tagIds = anchorCourse.CourseTags?
                    .Select(ct => ct.TagId)
                    .Distinct()
                    .ToList() ?? new List<Guid>();

                if (tagIds.Count > 0)
                {
                    becauseCandidateIds = await _unitOfWork.Courses.GetCoursesBySimilarTagsAsync(
                        tagIds,
                        Math.Max(top * 6, 20),
                        cancellationToken);
                }

                if (becauseCandidateIds.Count == 0)
                {
                    _logger.LogInformation(
                        "Because-you-learned fallback to recommendation service. UserId: {UserId}, AnchorCourseId: {AnchorCourseId}",
                        userId,
                        anchorCourse.Id);

                    becauseCandidateIds = await _recommendationService.GetRecommendedCoursesAsync(
                        userId,
                        Math.Max(top * 6, 20),
                        cancellationToken);
                }

                if (becauseCandidateIds.Count == 0)
                {
                    _logger.LogInformation(
                        "Because-you-learned fallback to top popular courses. UserId: {UserId}, AnchorCourseId: {AnchorCourseId}",
                        userId,
                        anchorCourse.Id);

                    becauseCandidateIds = await _unitOfWork.Courses.GetTopPopularCoursesAsync(
                        Math.Max(top * 6, 20),
                        cancellationToken);
                }

                var becauseStrictExclusions = new HashSet<Guid>(enrolledCourseIds)
                {
                    anchorCourse.Id
                };

                foreach (var course in topPicksForYou)
                {
                    becauseStrictExclusions.Add(course.CourseId);
                }

                becauseYouLearned = await BuildOrderedCourseDtosAsync(
                    becauseCandidateIds,
                    becauseStrictExclusions,
                    top,
                    cancellationToken);

                if (becauseYouLearned.Count == 0)
                {
                    _logger.LogInformation(
                        "Because-you-learned strict exclusions yielded no courses. Retrying without top-picks exclusion. UserId: {UserId}, AnchorCourseId: {AnchorCourseId}",
                        userId,
                        anchorCourse.Id);

                    var becauseLooseExclusions = new HashSet<Guid>(enrolledCourseIds)
                    {
                        anchorCourse.Id
                    };

                    becauseYouLearned = await BuildOrderedCourseDtosAsync(
                        becauseCandidateIds,
                        becauseLooseExclusions,
                        top,
                        cancellationToken);

                    if (becauseYouLearned.Count == 0)
                    {
                        var fallbackCandidates = await _unitOfWork.Courses.GetTopPopularCoursesAsync(
                            Math.Max(top * 6, 20),
                            cancellationToken);

                        becauseYouLearned = await BuildOrderedCourseDtosAsync(
                            fallbackCandidates,
                            becauseLooseExclusions,
                            top,
                            cancellationToken);
                    }
                }
            }

            var topCourseExclusions = new HashSet<Guid>(enrolledCourseIds);
            foreach (var course in topPicksForYou)
            {
                topCourseExclusions.Add(course.CourseId);
            }
            foreach (var course in becauseYouLearned)
            {
                topCourseExclusions.Add(course.CourseId);
            }

            var topPopularCandidates = await _unitOfWork.Courses.GetTopPopularCoursesAsync(
                Math.Max(top * 5, 20),
                cancellationToken);

            var topCourses = await BuildOrderedCourseDtosAsync(
                topPopularCandidates,
                topCourseExclusions,
                top,
                cancellationToken);

            var trendingExclusions = new HashSet<Guid>(topCourseExclusions);
            foreach (var course in topCourses)
            {
                trendingExclusions.Add(course.CourseId);
            }

            var trendingCourses = await BuildTrendingCoursesAsync(trendingExclusions, top, cancellationToken);

            _logger.LogInformation(
                "Recommendation sections built. UserId: {UserId}, TopPicks: {TopPicks}, Continue: {Continue}, Because: {Because}, TopCourses: {TopCourses}, Trending: {Trending}",
                userId,
                topPicksForYou.Count,
                continueLearning.Count,
                becauseYouLearned.Count,
                topCourses.Count,
                trendingCourses.Count);

            return new RecommendationSectionsDto
            {
                TopPicksForYou = topPicksForYou,
                ContinueLearning = continueLearning,
                BecauseYouLearnedCourseTitle = anchorCourse?.Title,
                BecauseYouLearned = becauseYouLearned,
                TopCourses = topCourses,
                TrendingCourses = trendingCourses
            };
        }

        private async Task<Course?> ResolveAnchorCourseAsync(
            IEnumerable<Enrollment> enrollments,
            CancellationToken cancellationToken)
        {
            var orderedCourseIds = enrollments
                .Where(e => e.Status != EnrollmentStatus.Dropped)
                .OrderByDescending(e => e.Status == EnrollmentStatus.Completed)
                .ThenByDescending(e => e.EnrolledAt)
                .Select(e => e.CourseId)
                .Distinct()
                .ToList();

            if (orderedCourseIds.Count == 0)
            {
                return null;
            }

            Course? fallbackCourse = null;

            foreach (var courseId in orderedCourseIds)
            {
                var course = await _unitOfWork.Courses.GetCourseByIdAsync(
                    courseId,
                    new CourseIncludeOptions
                    {
                        IncludeCourseTags = true
                    },
                    cancellationToken);

                if (course == null)
                {
                    continue;
                }

                fallbackCourse ??= course;

                if (course.CourseTags != null && course.CourseTags.Count > 0)
                {
                    return course;
                }
            }

            return fallbackCourse;
        }

        private async Task<List<CourseListDto>> BuildOrderedCourseDtosAsync(
            IEnumerable<Guid> orderedCourseIds,
            ISet<Guid> excludedCourseIds,
            int take,
            CancellationToken cancellationToken)
        {
            if (take <= 0)
            {
                return new List<CourseListDto>();
            }

            var ids = orderedCourseIds
                .Where(id => id != Guid.Empty && !excludedCourseIds.Contains(id))
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new List<CourseListDto>();
            }

            var courses = await _unitOfWork.Courses.GetSelectedCoursesAsync(
                ids,
                cancellationToken,
                new CourseIncludeOptions
                {
                    IncludeTeacher = true,
                    IncludeLectures = true,
                    IncludeEnrollments = true,
                    IncludeReviews = true,
                    IncludeCategories = true
                });

            var lookup = courses
                .Where(c => c.IsPublished)
                .ToDictionary(c => c.Id, c => c);

            var result = new List<CourseListDto>(Math.Min(take, ids.Count));

            foreach (var id in ids)
            {
                if (!lookup.TryGetValue(id, out var course))
                {
                    continue;
                }

                result.Add(MapCourseToListDto(course));
                if (result.Count >= take)
                {
                    break;
                }
            }

            return result;
        }

        private async Task<List<CourseListDto>> BuildTrendingCoursesAsync(
            ISet<Guid> excludedCourseIds,
            int top,
            CancellationToken cancellationToken)
        {
            var candidateTake = Math.Max(top * 4, 20);

            var popularIds = await _unitOfWork.Courses.GetTopPopularCoursesAsync(candidateTake, cancellationToken);
            var newestIds = await _unitOfWork.Courses.GetNewestCoursesAsync(candidateTake, cancellationToken);

            var candidateIds = popularIds
                .Concat(newestIds)
                .Where(id => id != Guid.Empty && !excludedCourseIds.Contains(id))
                .Distinct()
                .ToList();

            if (candidateIds.Count == 0)
            {
                return new List<CourseListDto>();
            }

            var popularity = await _unitOfWork.Courses.GetCoursePopularityAsync(candidateIds, cancellationToken);
            var recency = await _unitOfWork.Courses.GetCourseRecencyAsync(candidateIds, cancellationToken);

            var popularityById = popularity
                .ToDictionary(p => p.CourseId, p => (double)p.EnrollmentCount);

            var minPopularity = popularityById.Count > 0 ? popularityById.Values.Min() : 0d;
            var maxPopularity = popularityById.Count > 0 ? popularityById.Values.Max() : 1d;

            var recencyById = recency.ToDictionary(
                r => r.CourseId,
                r => Math.Exp(-Math.Max((DateTime.UtcNow - r.CreatedAt).TotalDays, 0d) / 45d));

            var rankedIds = candidateIds
                .Select(courseId => new
                {
                    CourseId = courseId,
                    Score =
                        0.65 * Normalize(popularityById.TryGetValue(courseId, out var p) ? p : minPopularity, minPopularity, maxPopularity)
                        + 0.35 * (recencyById.TryGetValue(courseId, out var r) ? r : 0d)
                })
                .OrderByDescending(x => x.Score)
                .Select(x => x.CourseId)
                .ToList();

            return await BuildOrderedCourseDtosAsync(rankedIds, excludedCourseIds, top, cancellationToken);
        }

        private static double Normalize(double value, double min, double max)
        {
            if (max <= min)
            {
                return 1d;
            }

            return (value - min) / (max - min);
        }

        private static CourseListDto MapCourseToListDto(Course course)
        {
            var firstCategory = course.CourseCategories?.FirstOrDefault();

            return new CourseListDto
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
            };
        }
    }
}