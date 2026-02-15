using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses
{
    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, PagedResult<CourseListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetAllCoursesQueryHandler> _logger;

        public GetAllCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetAllCoursesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<CourseListDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all courses. OnlyPublished: {OnlyPublished}, Page: {Page}", request.OnlyPublished, request.Page);

            var (courses, totalCount) = await _unitOfWork.Courses.GetCoursesPagedAsync(
                request.OnlyPublished,
                request.Page,
                request.PageSize,
                cancellationToken);

            // Get enrolled course IDs for the current user
            var enrolledCourseIds = new HashSet<Guid>();
            var userId = _currentUserService.UserId;
            if (userId.HasValue)
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                    userId.Value,
                    includeCourse: false,
                    cancellationToken);
                enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();
            }

            var items = courses.Select(c => new CourseListDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                TeacherId = c.TeacherId,
                TeacherName = c.Teacher?.UserName ?? string.Empty,
                IsPublished = c.IsPublished,
                LectureCount = c.Lectures?.Count ?? 0,
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                CreatedAt = c.CreatedAt,
                IsEnrolled = enrolledCourseIds.Contains(c.Id),
                AverageRating = c.Reviews != null && c.Reviews.Count > 0 ? Math.Round(c.Reviews.Average(r => r.Rating), 2) : 0,
                ReviewCount = c.Reviews?.Count ?? 0
            }).ToList();

            _logger.LogInformation("Retrieved {Count}/{Total} courses", items.Count, totalCount);

            return new PagedResult<CourseListDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
