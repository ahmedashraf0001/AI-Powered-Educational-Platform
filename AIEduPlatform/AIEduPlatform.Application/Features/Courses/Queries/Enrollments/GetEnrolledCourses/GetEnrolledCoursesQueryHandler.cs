using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
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

            var user = await _unitOfWork.Users.GetUserByIdAsync(studentId.Value, ct: cancellationToken);
            var studentName = user != null
                ? $"{user.FirstName} {user.LastName}".Trim()
                : string.Empty;

            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByStudentAsync(
                studentId.Value,
                includeCourse: true,
                cancellationToken);

            var items = new List<EnrollmentDto>();
            foreach (var e in enrollments)
            {
                var courseId = e.CourseId;
                var course = e.Course;

                var teacherName = string.Empty;
                if (course?.Teacher != null)
                {
                    teacherName = $"{course.Teacher.FirstName} {course.Teacher.LastName}".Trim();
                    if (string.IsNullOrWhiteSpace(teacherName))
                    {
                        teacherName = course.Teacher.UserName ?? string.Empty;
                    }
                }

                var lectures = course?.Lectures?.ToList() ?? [];
                var totalLectures = lectures.Count;

                var progressRows = await _unitOfWork.MaterialProgress.GetProgressByCourseAsync(
                    studentId.Value,
                    courseId,
                    cancellationToken);

                var completedMaterialIds = progressRows
                    .Where(p => p.IsCompleted)
                    .Select(p => p.MaterialId)
                    .ToHashSet();

                var completedLectures = totalLectures > 0
                    ? lectures.Count(l =>
                        l.Materials != null
                        && l.Materials.Any()
                        && l.Materials.All(m => completedMaterialIds.Contains(m.Id)))
                    : 0;

                var lastAccessedAt = progressRows
                    .Where(p => !p.IsCompleted)
                    .OrderByDescending(p => p.UpdatedAt)
                    .Select(p => (DateTime?)p.UpdatedAt)
                    .FirstOrDefault();

                var progressPct = totalLectures > 0
                    ? Math.Round((double)completedLectures / totalLectures * 100, 1)
                    : 0;

                items.Add(new EnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = studentName,
                    CourseId = e.CourseId,
                    CourseTitle = course?.Title ?? string.Empty,
                    TeacherName = teacherName,
                    CourseThumbnailUrl = course?.ThumbnailUrl,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status,
                    ProgressPercentage = progressPct,
                    CompletedLectures = completedLectures,
                    TotalLectures = totalLectures,
                    LastAccessedAt = lastAccessedAt,
                    IsCompleted = e.Status == Core.Domain.Enums.EnrollmentStatus.Completed
                        || (totalLectures > 0 && completedLectures >= totalLectures),
                    OrderId = e.OrderId,
                    AmountPaid = e.AmountPaid,
                    RefundedAt = e.RefundedAt,
                    RefundAmount = e.RefundAmount,
                    StripeRefundId = e.StripeRefundId,
                    UnenrolledAt = e.UnenrolledAt
                });
            }

            var query = items.AsEnumerable();

            if (!request.ShowDropped)
            {
                query = query.Where(i => i.Status != Core.Domain.Enums.EnrollmentStatus.Dropped);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                var search = request.SearchQuery.Trim().ToLowerInvariant();
                query = query.Where(i => !string.IsNullOrEmpty(i.CourseTitle) && i.CourseTitle.ToLowerInvariant().Contains(search));
            }

            query = request.SortBy?.ToLowerInvariant() switch
            {
                "enrolled" => query.OrderByDescending(i => i.EnrolledAt),
                "progress" => query.OrderByDescending(i => i.ProgressPercentage),
                "title" => query.OrderBy(i => i.CourseTitle),
                "accessed" => query.OrderByDescending(i => i.LastAccessedAt ?? DateTime.MinValue),
                _ => query.OrderByDescending(i => i.LastAccessedAt ?? DateTime.MinValue)
            };

            var filteredItems = query.ToList();
            var totalCount = filteredItems.Count;

            var pagedItems = filteredItems
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<EnrollmentDto>
            {
                Items = pagedItems,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };
        }
    }
}
