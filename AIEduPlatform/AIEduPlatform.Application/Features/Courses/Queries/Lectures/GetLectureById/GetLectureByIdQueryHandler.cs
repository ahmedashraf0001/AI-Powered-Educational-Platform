using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Lectures.GetLectureById
{
    public class GetLectureByIdQueryHandler : IRequestHandler<GetLectureByIdQuery, LectureDetailDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ILogger<GetLectureByIdQueryHandler> _logger;

        public GetLectureByIdQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            ILogger<GetLectureByIdQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _logger = logger;
        }

        public async Task<LectureDetailDto> Handle(GetLectureByIdQuery request, CancellationToken cancellationToken)
        {
            var lecture = await _unitOfWork.Lectures.GetLectureByIdAsync(
                request.LectureId, includeMaterials: true, ct: cancellationToken);

            if (lecture is null)
                throw new NotFoundException(nameof(Lecture), request.LectureId);

            // Check enrollment or instructor access
            var userId = _currentUser.UserId;
            if (userId is null || userId == Guid.Empty)
                throw new ForbiddenException("You must be logged in to view lecture details.");

            var course = await _unitOfWork.Courses.GetByIdAsync(lecture.CourseId, cancellationToken);
            if (course is null)
                throw new NotFoundException(nameof(Course), lecture.CourseId);

            var isInstructor = course.TeacherId == userId;
            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value, lecture.CourseId, cancellationToken);

            if (!isInstructor && !isEnrolled)
                throw new ForbiddenException("You must be enrolled in the course to view lecture materials.");

            // Group materials by type
            var materialsByType = (lecture.Materials ?? Enumerable.Empty<Material>())
                .GroupBy(m => m.Type.ToString())
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(m => new MaterialDto
                    {
                        Id = m.Id,
                        LectureId = m.LectureId,
                        Type = m.Type,
                        Title = m.Title,
                        StreamUrl = $"/api/materials/{m.Id}/stream",
                        Indexed = m.Indexed,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    }).ToList()
                );

            var totalMaterials = lecture.Materials?.Count ?? 0;

            _logger.LogInformation(
                "Retrieved lecture {LectureId} with {MaterialCount} materials for user {UserId}",
                request.LectureId, totalMaterials, userId);

            return new LectureDetailDto
            {
                Id = lecture.Id,
                CourseId = lecture.CourseId,
                CourseTitle = course.Title,
                Title = lecture.Title,
                Description = lecture.Description,
                OrderIndex = lecture.OrderIndex,
                CreatedAt = lecture.CreatedAt,
                UpdatedAt = lecture.UpdatedAt,
                MaterialsByType = materialsByType,
                TotalMaterials = totalMaterials
            };
        }
    }
}
