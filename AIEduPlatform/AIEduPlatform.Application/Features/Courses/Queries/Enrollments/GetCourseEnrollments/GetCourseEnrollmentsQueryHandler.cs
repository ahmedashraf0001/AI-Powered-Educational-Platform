using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments
{
    public class GetCourseEnrollmentsQueryHandler : IRequestHandler<GetCourseEnrollmentsQuery, List<EnrollmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCourseEnrollmentsQueryHandler> _logger;

        public GetCourseEnrollmentsQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetCourseEnrollmentsQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<EnrollmentDto>> Handle(GetCourseEnrollmentsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view course enrollments.");
            }

            _logger.LogInformation(
                "Getting enrollments for course: {CourseId}, UserId: {UserId}",
                request.CourseId,
                userId.Value);

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning(
                    "User {UserId} is not authorized to view enrollments for course {CourseId}",
                    userId.Value,
                    request.CourseId);
                throw new ForbiddenException("You are not authorized to view enrollments for this course.");
            }

            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByCourseAsync(
                request.CourseId,
                includeStudent: true,
                cancellationToken);

            var result = enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student?.UserName ?? string.Empty,
                CourseId = e.CourseId,
                CourseTitle = course.Title,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} enrollments for course {CourseId}",
                result.Count,
                request.CourseId);

            return result;
        }
    }
}
