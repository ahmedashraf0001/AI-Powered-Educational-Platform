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

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view course details.");
            }

            _logger.LogInformation(
                "Getting course by ID: {CourseId}, UserId: {UserId}",
                request.CourseId,
                userId.Value);

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            var isInstructor = course.TeacherId == userId.Value;
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value,
                request.CourseId,
                cancellationToken);

            if (!isInstructor && !isEnrolled)
            {
                _logger.LogWarning(
                    "User {UserId} is not authorized to view details of course {CourseId}",
                    userId.Value,
                    request.CourseId);
                throw new ForbiddenException("You must be enrolled in this course or be the instructor to view details.");
            }

            var options = new CourseIncludeOptions
            {
                IncludeLectures = request.IncludeLectures,
                IncludeMaterials = request.IncludeMaterials,
                IncludeEnrollments = isInstructor
            };

            var fullCourse = await _unitOfWork.Courses.GetCourseByIdAsync(request.CourseId, options, cancellationToken);

            var result = new CourseDetailDto
            {
                Id = fullCourse!.Id,
                Title = fullCourse.Title,
                Description = fullCourse.Description,
                TeacherId = fullCourse.TeacherId,
                TeacherName = fullCourse.Teacher?.UserName ?? string.Empty,
                IsPublished = fullCourse.IsPublished,
                CreatedAt = fullCourse.CreatedAt,
                UpdatedAt = fullCourse.UpdatedAt,
                EnrollmentCount = fullCourse.Enrollments?.Count ?? 0,
                Lectures = fullCourse.Lectures?.OrderBy(l => l.OrderIndex).Select(l => new LectureDto
                {
                    Id = l.Id,
                    CourseId = l.CourseId,
                    Title = l.Title,
                    Description = l.Description,
                    OrderIndex = l.OrderIndex,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt,
                    Materials = l.Materials?.Select(m => new MaterialDto
                    {
                        Id = m.Id,
                        LectureId = m.LectureId,
                        Type = m.Type,
                        Title = m.Title,
                        FileUrl = m.FileUrl,
                        Indexed = m.Indexed,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    }).ToList() ?? []
                }).ToList() ?? []
            };

            _logger.LogInformation(
                "Successfully retrieved course. CourseId: {CourseId}, Title: {Title}",
                fullCourse.Id,
                fullCourse.Title);

            return result;
        }
    }
}
