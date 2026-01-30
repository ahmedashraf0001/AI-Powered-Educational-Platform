using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Courses.SearchCourses
{
    public class SearchCoursesQueryHandler : IRequestHandler<SearchCoursesQuery, List<CourseListDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SearchCoursesQueryHandler> _logger;

        public SearchCoursesQueryHandler(
            IUnitOfWork unitOfWork,
            ILogger<SearchCoursesQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<List<CourseListDto>> Handle(SearchCoursesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "Searching courses with keyword: {Keyword}, OnlyPublished: {OnlyPublished}",
                request.Keyword,
                request.OnlyPublished);

            var courses = await _unitOfWork.Courses.SearchCoursesByKeywordAsync(
                request.Keyword,
                null,
                cancellationToken);

            if (courses == null)
            {
                return [];
            }

            if (request.OnlyPublished)
            {
                courses = courses.Where(c => c.IsPublished).ToList();
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
                "Found {Count} courses matching keyword: {Keyword}",
                result.Count,
                request.Keyword);

            return result;
        }
    }
}
