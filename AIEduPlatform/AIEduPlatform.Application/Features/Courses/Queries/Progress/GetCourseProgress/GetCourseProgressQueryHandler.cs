using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.DTOs.Progress;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;

namespace AIEduPlatform.Application.Features.Courses.Queries.Progress.GetCourseProgress
{
    public class GetCourseProgressQueryHandler : IRequestHandler<GetCourseProgressQuery, CourseProgressDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public GetCourseProgressQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<CourseProgressDto> Handle(GetCourseProgressQuery request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in.");

            var course = await _unitOfWork.Courses.GetCourseByIdAsync(request.CourseId, ct: cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(studentId, request.CourseId, cancellationToken);
            if (enrollment == null)
                throw new BadRequestException("You are not enrolled in this course.");

            var totalMaterials = await _unitOfWork.Courses.GetMaterialsCountAsync(request.CourseId, cancellationToken);
            var completedMaterials = await _unitOfWork.MaterialProgress.GetCompletedMaterialCountAsync(studentId, request.CourseId, cancellationToken);

            var progressPercentage = totalMaterials > 0
                ? Math.Round((double)completedMaterials / totalMaterials * 100, 1)
                : 0;

            return new CourseProgressDto
            {
                CourseId = course.Id,
                CourseTitle = course.Title,
                CompletedLessons = completedMaterials,
                TotalLessons = totalMaterials,
                ProgressPercentage = progressPercentage,
                IsCompleted = enrollment.Status == Core.Domain.Enums.EnrollmentStatus.Completed
            };
        }
    }
}

