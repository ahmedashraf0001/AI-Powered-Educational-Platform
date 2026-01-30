using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.GetAllCourses
{
    public class GetAllCoursesQueryHandler : IRequestHandler<GetAllCoursesQuery, List<CourseListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<GetAllCoursesQueryHandler> _logger;

        public GetAllCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<GetAllCoursesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<CourseListDto>> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all courses. OnlyPublished: {OnlyPublished}", request.OnlyPublished);

            var courses = await _unitOfWork.Courses.GetAllAsync(cancellationToken);

            if (request.OnlyPublished)
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

            _logger.LogInformation("Retrieved {Count} courses", result.Count);

            return result;
        }
    }
}
