using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCourseById
{
    public class GetCourseByIdQueryHandler : IRequestHandler<GetCourseByIdQuery, CourseDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCourseByIdQueryHandler> _logger;

        public GetCourseByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetCourseByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CourseDetailDto> Handle(GetCourseByIdQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            _logger.LogInformation(
                "Getting course by ID: {CourseId}, UserId: {UserId}",
                request.CourseId, userId);

            var options = new CourseIncludeOptions
            {
                IncludeLectures = true,
                IncludeMaterials = false,
                IncludeEnrollments = true,
                IncludeTeacher = true,
                IncludeReviews = false,
                IncludeCategories = true
            };

            var course = await _unitOfWork.Courses.GetCourseByIdAsync(request.CourseId, options, cancellationToken);

            if (course == null)
                throw new NotFoundException(nameof(Course), request.CourseId);

            var isInstructor = userId.HasValue && course.TeacherId == userId.Value;
            var isEnrolled = userId.HasValue && await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (!course.IsPublished && !isInstructor)
                throw new NotFoundException(nameof(Course), request.CourseId);

            // Check if user has already reviewed
            var hasReviewed = userId.HasValue && await _unitOfWork.Reviews.HasStudentReviewedAsync(
                userId.Value, request.CourseId, cancellationToken);

            // Get rating summary
            var (averageRating, totalReviews, _) = await _unitOfWork.Reviews
                .GetCourseRatingSummaryAsync(request.CourseId, cancellationToken);

            var firstCategory = course.CourseCategories?.FirstOrDefault();

            var result = new CourseDetailDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                TeacherId = course.TeacherId,
                TeacherName = course.Teacher?.UserName ?? string.Empty,
                IsPublished = course.IsPublished,
                CreatedAt = course.CreatedAt,
                UpdatedAt = course.UpdatedAt,
                LectureCount = course.Lectures?.Count ?? 0,
                EnrollmentCount = course.Enrollments?.Count ?? 0,
                IsEnrolled = isEnrolled,
                HasReviewed = hasReviewed,
                AverageRating = totalReviews > 0 ? Math.Round(averageRating, 2) : 0,
                ReviewCount = totalReviews,
                CategoryId = firstCategory?.CategoryId,
                CategoryName = firstCategory?.Category?.Name,
                Price = course.Price,
                IsFree = course.Price == 0,
                ThumbnailUrl = course.ThumbnailUrl,
                Lectures = course.Lectures?.OrderBy(l => l.OrderIndex).Select(l => new LectureSummaryDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    OrderIndex = l.OrderIndex
                }).ToList() ?? []
            };

            _logger.LogInformation(
                "Retrieved course {CourseId}. Enrolled: {IsEnrolled}, Instructor: {IsInstructor}",
                course.Id, isEnrolled, isInstructor);

            return result;
        }
    }
}
