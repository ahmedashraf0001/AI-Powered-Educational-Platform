using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent
{
    public class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<UnenrollStudentCommandHandler> _logger;

        public UnenrollStudentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<UnenrollStudentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId;

            if (!studentId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to unenroll from a course.");
            }

            _logger.LogInformation(
                "Unenrolling student from course. StudentId: {StudentId}, CourseId: {CourseId}",
                studentId.Value,
                request.CourseId);

            try
            {
                var enrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                    studentId.Value,
                    request.CourseId,
                    cancellationToken);

                if (enrollment == null)
                {
                    _logger.LogWarning(
                        "Enrollment not found. StudentId: {StudentId}, CourseId: {CourseId}",
                        studentId.Value,
                        request.CourseId);
                    throw new NotFoundException("You are not enrolled in this course.");
                }

                enrollment.Status = EnrollmentStatus.Dropped;
                enrollment.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.Enrollments.UpdateAsync(enrollment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully unenrolled student. EnrollmentId: {EnrollmentId}, StudentId: {StudentId}, CourseId: {CourseId}",
                    enrollment.Id,
                    studentId.Value,
                    request.CourseId);

                // Notify teacher about student dropping the course
                var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);
                var student = await _unitOfWork.Users.GetUserByIdAsync(studentId.Value, ct: cancellationToken);
                if (course != null)
                {
                    await _notificationService.NotifyStudentUnenrolledAsync(
                        course.TeacherId,
                        student?.FirstName ?? "A student",
                        course.Title,
                        cancellationToken);
                }

                return Unit.Value;
            }
            catch (Exception ex) when (ex is not NotFoundException and not UnauthorizedException)
            {
                _logger.LogError(
                    ex,
                    "Error unenrolling student. StudentId: {StudentId}, CourseId: {CourseId}",
                    studentId.Value,
                    request.CourseId);

                throw;
            }
        }
    }
}
