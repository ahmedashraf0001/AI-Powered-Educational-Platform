using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent
{
    public class EnrollStudentCommandHandler : IRequestHandler<EnrollStudentCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<EnrollStudentCommandHandler> _logger;

        public EnrollStudentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<EnrollStudentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Guid> Handle(EnrollStudentCommand request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId;

            if (!studentId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to enroll in a course.");
            }

            _logger.LogInformation(
                "Enrolling student in course. StudentId: {StudentId}, CourseId: {CourseId}",
                studentId.Value,
                request.CourseId);

            try
            {
                var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

                if (course == null)
                {
                    _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                    throw new NotFoundException(nameof(Course), request.CourseId);
                }

                if (!course.IsPublished)
                {
                    _logger.LogWarning(
                        "Cannot enroll in unpublished course. CourseId: {CourseId}",
                        request.CourseId);
                    throw new BadRequestException("Cannot enroll in an unpublished course.");
                }

                if (course.TeacherId == studentId.Value)
                {
                    throw new BadRequestException("Instructors cannot enroll in their own courses.");
                }

                // Check for any existing enrollment (including dropped/completed)
                var existingEnrollmentRecord = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                    studentId.Value,
                    request.CourseId,
                    cancellationToken);

                Enrollment createdEnrollment;

                if (existingEnrollmentRecord != null)
                {
                    if (existingEnrollmentRecord.Status == EnrollmentStatus.Active)
                    {
                        _logger.LogWarning(
                            "Student already enrolled. StudentId: {StudentId}, CourseId: {CourseId}",
                            studentId.Value,
                            request.CourseId);
                        throw new BadRequestException("You are already enrolled in this course.");
                    }

                    // Reactivate a dropped/completed enrollment
                    existingEnrollmentRecord.Status = EnrollmentStatus.Active;
                    existingEnrollmentRecord.EnrolledAt = DateTime.UtcNow;
                    existingEnrollmentRecord.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.Enrollments.UpdateAsync(existingEnrollmentRecord, cancellationToken);
                    createdEnrollment = existingEnrollmentRecord;
                }
                else
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = studentId.Value,
                        CourseId = request.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        Status = EnrollmentStatus.Active,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    createdEnrollment = await _unitOfWork.Enrollments.AddAsync(enrollment, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully enrolled student. EnrollmentId: {EnrollmentId}, StudentId: {StudentId}, CourseId: {CourseId}",
                    createdEnrollment.Id,
                    studentId.Value,
                    request.CourseId);

                // Notify teacher about new enrollment
                var student = await _unitOfWork.Users.GetUserByIdAsync(studentId.Value, ct: cancellationToken);
                await _notificationService.NotifyNewEnrollmentAsync(
                    course.TeacherId,
                    student?.FirstName ?? "A student",
                    course.Title,
                    cancellationToken);

                return createdEnrollment.Id;
            }
            catch (Exception ex) when (ex is not NotFoundException and not BadRequestException and not UnauthorizedException)
            {
                _logger.LogError(
                    ex,
                    "Error enrolling student. StudentId: {StudentId}, CourseId: {CourseId}",
                    studentId.Value,
                    request.CourseId);

                throw;
            }
        }
    }
}
