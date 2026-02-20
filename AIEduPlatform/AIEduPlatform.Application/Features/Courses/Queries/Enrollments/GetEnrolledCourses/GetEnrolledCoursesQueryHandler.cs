using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetEnrolledCourses
{
    public class GetEnrolledCoursesQueryHandler : IRequestHandler<GetEnrolledCoursesQuery, PagedResult<EnrollmentDto>>
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

        public async Task<PagedResult<EnrollmentDto>> Handle(GetEnrolledCoursesQuery request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId;

            if (!studentId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view your enrolled courses.");
            }

            var (enrollments, totalCount) = await _unitOfWork.Enrollments.GetPagedAsync(
                e => e.StudentId == studentId.Value,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

            var items = enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = string.Empty,
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? string.Empty,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status
            }).ToList();

            return new PagedResult<EnrollmentDto>
            {
                Items = items,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
