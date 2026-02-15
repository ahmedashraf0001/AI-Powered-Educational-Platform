using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureMaterials
{
    public class GetLectureMaterialsQueryHandler : IRequestHandler<GetLectureMaterialsQuery, List<MaterialDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetLectureMaterialsQueryHandler> _logger;

        public GetLectureMaterialsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetLectureMaterialsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<MaterialDto>> Handle(GetLectureMaterialsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            _logger.LogInformation(
                "Getting materials for lecture: {LectureId}, UserId: {UserId}",
                request.LectureId,
                userId);

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to access lecture materials.");
            }

            var lecture = await _unitOfWork.Lectures.GetLectureByIdAsync(
                request.LectureId,
                includeMaterials: true,
                cancellationToken);

            if (lecture == null)
            {
                _logger.LogWarning("Lecture not found. LectureId: {LectureId}", request.LectureId);
                throw new NotFoundException(nameof(Lecture), request.LectureId);
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found. CourseId: {CourseId}", lecture.CourseId);
                throw new NotFoundException(nameof(Course), lecture.CourseId);
            }

            var isInstructor = course.TeacherId == userId.Value;
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value,
                lecture.CourseId,
                cancellationToken);

            if (!isInstructor && !isEnrolled)
            {
                _logger.LogWarning(
                    "User {UserId} is not authorized to access materials for lecture {LectureId}",
                    userId,
                    request.LectureId);
                throw new ForbiddenException("You must be enrolled in this course or be the instructor to access materials.");
            }

            var result = lecture.Materials?.Select(m => new MaterialDto
            {
                Id = m.Id,
                LectureId = m.LectureId,
                Type = m.Type,
                Title = m.Title,
                StreamUrl = $"/api/materials/{m.Id}/stream",
                Indexed = m.Indexed,
                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt
            }).ToList() ?? [];

            _logger.LogInformation(
                "Retrieved {Count} materials for lecture {LectureId}",
                result.Count,
                request.LectureId);

            return result;
        }
    }
}
