using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetCoursesByInstructor
{
    public class GetCoursesByInstructorQueryHandler : IRequestHandler<GetCoursesByInstructorQuery, List<CourseListDto>>
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

        public async Task<List<CourseListDto>> Handle(GetCoursesByInstructorQuery request, CancellationToken cancellationToken)
        {
            var instructorId = request.InstructorId ?? _currentUserService.UserId;

            if (!instructorId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view courses.");
            }

            _logger.LogInformation(
                "Getting courses by instructor. InstructorId: {InstructorId}, IncludeUnpublished: {IncludeUnpublished}",
                instructorId.Value,
                request.IncludeUnpublished);

            var courses = await _unitOfWork.Courses.FindAsync(
                c => c.TeacherId == instructorId.Value,
                cancellationToken);

            if (!request.IncludeUnpublished)
            {
                courses = courses.Where(c => c.IsPublished);
            }

            var result = courses.Select(c => new CourseListDto
            {
                Id = c.Id,
                Title = c.Title,
                Description = c.Description,
                TeacherId = c.TeacherId,
                IsPublished = c.IsPublished,
                LectureCount = c.Lectures?.Count ?? 0,
                EnrollmentCount = c.Enrollments?.Count ?? 0,
                CreatedAt = c.CreatedAt
            }).ToList();

            _logger.LogInformation(
                "Retrieved {Count} courses for instructor {InstructorId}",
                result.Count,
                instructorId.Value);

            return result;
        }
    }
}
