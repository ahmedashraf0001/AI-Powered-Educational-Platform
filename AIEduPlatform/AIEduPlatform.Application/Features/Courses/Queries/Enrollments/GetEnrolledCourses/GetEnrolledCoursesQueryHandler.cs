using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public class GetEnrolledCoursesQueryHandler : IRequestHandler<GetEnrolledCoursesQuery, List<EnrollmentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetEnrolledCoursesQueryHandler> _logger;

        public GetEnrolledCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<GetEnrolledCoursesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<List<EnrollmentDto>> Handle(GetEnrolledCoursesQuery request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId;

            if (!studentId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view your enrolled courses.");
            }

            _logger.LogInformation("Getting enrolled courses for student: {StudentId}", studentId.Value);

            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                studentId.Value,
                includeCourse: true,
                cancellationToken);

            var result = enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = string.Empty,
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? string.Empty,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} enrolled courses for student {StudentId}",
                result.Count,
                studentId.Value);

            return result;
        }
    }
}
