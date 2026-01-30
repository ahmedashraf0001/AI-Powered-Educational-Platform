using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetCourseLectures
{
    public class GetCourseLecturesQueryHandler : IRequestHandler<GetCourseLecturesQuery, List<LectureDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCourseLecturesQueryHandler> _logger;

        public GetCourseLecturesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetCourseLecturesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<LectureDto>> Handle(GetCourseLecturesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            _logger.LogInformation(
                "Getting lectures for course: {CourseId}, UserId: {UserId}",
                request.CourseId,
                userId);

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to access course lectures.");
            }

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
                    "User {UserId} is not authorized to access lectures for course {CourseId}",
                    userId,
                    request.CourseId);
                throw new ForbiddenException("You must be enrolled in this course or be the instructor to access lectures.");
            }

            var lectures = await _unitOfWork.Lectures.GetLecturesByCourseIdAsync(
                request.CourseId,
                request.IncludeMaterials,
                cancellationToken);

            var result = lectures.OrderBy(l => l.OrderIndex).Select(l => new LectureDto
            {
                Id = l.Id,
                CourseId = l.CourseId,
                Title = l.Title,
                Description = l.Description,
                OrderIndex = l.OrderIndex,
                CreatedAt = l.CreatedAt,
                UpdatedAt = l.UpdatedAt,
                Materials = request.IncludeMaterials
                    ? l.Materials?.Select(m => new MaterialDto
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
                    : []
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} lectures for course {CourseId}",
                result.Count,
                request.CourseId);

            return result;
        }
    }
}
