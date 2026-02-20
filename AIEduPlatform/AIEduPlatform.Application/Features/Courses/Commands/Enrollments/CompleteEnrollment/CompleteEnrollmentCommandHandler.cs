using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.CompleteEnrollment
{
    public class CompleteEnrollmentCommandHandler : IRequestHandler<CompleteEnrollmentCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<CompleteEnrollmentCommandHandler> _logger;

        public CompleteEnrollmentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<CompleteEnrollmentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(CompleteEnrollmentCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in.");

            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (enrollment is null)
                throw new NotFoundException("Enrollment", $"Student {userId.Value} in Course {request.CourseId}");

            if (enrollment.Status == EnrollmentStatus.Completed)
                throw new BadRequestException("This enrollment is already marked as completed.");

            if (enrollment.Status != EnrollmentStatus.Active)
                throw new BadRequestException("Only active enrollments can be marked as completed.");

            var updated = await _unitOfWork.Enrollments.UpdateEnrollmentStatusAsync(
                userId.Value, request.CourseId, EnrollmentStatus.Completed, cancellationToken);

            if (!updated)
                throw new BadRequestException("Failed to update enrollment status.");

            _logger.LogInformation(
                "Student {StudentId} completed course {CourseId}",
                userId.Value, request.CourseId);

            // Notify teacher about course completion
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);
            var student = await _unitOfWork.Users.GetUserByIdAsync(userId.Value, ct: cancellationToken);
            if (course != null)
            {
                await _notificationService.NotifyEnrollmentCompletedAsync(
                    course.TeacherId,
                    student?.FirstName ?? "A student",
                    course.Title,
                    cancellationToken);
            }

            return Unit.Value;
        }
    }
}
