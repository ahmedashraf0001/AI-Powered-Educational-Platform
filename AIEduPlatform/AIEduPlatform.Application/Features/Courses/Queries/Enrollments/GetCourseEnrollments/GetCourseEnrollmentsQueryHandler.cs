using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Common;
using AIEduPlatform.Core.DTOs.Courses;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Queries.Enrollments.GetCourseEnrollments
{
    public class GetCourseEnrollmentsQueryHandler : IRequestHandler<GetCourseEnrollmentsQuery, PagedResult<EnrollmentDto>>
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

        public async Task<PagedResult<EnrollmentDto>> Handle(GetCourseEnrollmentsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to view course enrollments.");
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

            if (course == null)
            {
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                throw new ForbiddenException("You are not authorized to view enrollments for this course.");
            }

            var (enrollments, totalCount) = await _unitOfWork.Enrollments.GetPagedAsync(
                e => e.CourseId == request.CourseId,
                request.Page,
                request.PageSize,
                cancellationToken: cancellationToken);

            var items = enrollments.Select(e => new EnrollmentDto
            {
                Id = e.Id,
                StudentId = e.StudentId,
                StudentName = e.Student?.UserName ?? string.Empty,
                CourseId = e.CourseId,
                CourseTitle = course.Title,
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
