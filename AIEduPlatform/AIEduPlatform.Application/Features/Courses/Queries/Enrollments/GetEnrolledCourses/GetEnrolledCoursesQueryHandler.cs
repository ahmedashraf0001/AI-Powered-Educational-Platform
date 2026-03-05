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

            var totalCount = enrollments.Count;
            var paged = enrollments
                .OrderByDescending(e => e.EnrolledAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var items = new List<EnrollmentDto>();
            foreach (var e in paged)
            {
                var courseId = e.CourseId;

                // Get total materials (lectures) for the course
                var totalMaterials = await _unitOfWork.Courses.GetMaterialsCountAsync(courseId, cancellationToken);
                var completedMaterials = await _unitOfWork.MaterialProgress.GetCompletedMaterialCountAsync(
                    studentId.Value, courseId, cancellationToken);
                var lastAccessed = await _unitOfWork.MaterialProgress.GetLastAccessedMaterialAsync(
                    studentId.Value, courseId, cancellationToken);

                var progressPct = totalMaterials > 0
                    ? Math.Round((double)completedMaterials / totalMaterials * 100, 1)
                    : 0;

                items.Add(new EnrollmentDto
                {
                    Id = e.Id,
                    StudentId = e.StudentId,
                    StudentName = studentName,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course?.Title ?? string.Empty,
                    EnrolledAt = e.EnrolledAt,
                    Status = e.Status,
                    ProgressPercentage = progressPct,
                    CompletedLectures = completedMaterials,
                    TotalLectures = totalMaterials,
                    LastAccessedAt = lastAccessed?.UpdatedAt,
                    IsCompleted = totalMaterials > 0 && completedMaterials >= totalMaterials,
                    OrderId = e.OrderId,
                    AmountPaid = e.AmountPaid,
                    RefundedAt = e.RefundedAt,
                    RefundAmount = e.RefundAmount,
                    StripeRefundId = e.StripeRefundId,
                    UnenrolledAt = e.UnenrolledAt
                });
            }

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
