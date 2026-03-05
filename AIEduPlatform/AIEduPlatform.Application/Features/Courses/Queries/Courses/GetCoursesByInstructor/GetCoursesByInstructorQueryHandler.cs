using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor
{
    public class GetCoursesByInstructorQueryHandler : IRequestHandler<GetCoursesByInstructorQuery, PagedResult<CourseListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCoursesByInstructorQueryHandler> _logger;

        public GetCoursesByInstructorQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetCoursesByInstructorQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<PagedResult<CourseListDto>> Handle(GetCoursesByInstructorQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            var instructorId = request.InstructorId ?? userId;

            if (!instructorId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view courses.");
            }

            _logger.LogInformation(
                "Getting courses by instructor. InstructorId: {InstructorId}, IncludeUnpublished: {IncludeUnpublished}",
                instructorId.Value,
                request.IncludeUnpublished);

            var (courses, totalCount) = await _unitOfWork.Courses.GetCoursesByInstructorPagedAsync(
                instructorId.Value,
                request.IncludeUnpublished,
                request.Page,
                request.PageSize,
                cancellationToken);

            // Get enrolled course IDs for the current user
            var enrolledCourseIds = new HashSet<Guid>();
            if (userId.HasValue)
            {
                var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                    userId.Value,
                    includeCourse: false,
                    cancellationToken);
                enrolledCourseIds = enrollments.Select(e => e.CourseId).ToHashSet();
            }

            var items = courses.Select(c =>
            {
                var firstCategory = c.CourseCategories?.FirstOrDefault();
                return new CourseListDto
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
                    ReviewCount = c.Reviews?.Count ?? 0,
                    CategoryId = firstCategory?.CategoryId,
                    CategoryName = firstCategory?.Category?.Name,
                    Price = c.Price,
                    IsFree = c.Price == 0,
                    ThumbnailUrl = c.ThumbnailUrl
                };
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count}/{Total} courses for instructor {InstructorId}",
                items.Count,
                totalCount,
                instructorId.Value);

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
