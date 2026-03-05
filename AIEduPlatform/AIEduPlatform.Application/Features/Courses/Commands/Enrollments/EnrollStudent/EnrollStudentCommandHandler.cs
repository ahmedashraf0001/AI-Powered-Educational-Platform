using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.EnrollStudent
{
    /// <summary>
    /// Direct enrollment endpoint — only for FREE courses (Price == 0).
    /// Paid courses must go through Cart → Checkout → Stripe Payment → Webhook.
    /// </summary>
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
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to enroll in a course.");

            _logger.LogInformation(
                "Enrolling student in course. StudentId: {StudentId}, CourseId: {CourseId}",
                studentId, request.CourseId);

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            if (!course.IsPublished)
                throw new BadRequestException("Cannot enroll in an unpublished course.");

            if (course.TeacherId == studentId)
                throw new BadRequestException("Instructors cannot enroll in their own courses.");

            // Paid courses must go through cart → checkout
            if (course.Price > 0)
                throw new BadRequestException("Paid courses must be enrolled via the checkout process. Add the course to your cart and proceed to checkout.");

            // Check if already enrolled
            var existingEnrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                studentId, request.CourseId, cancellationToken);

            if (existingEnrollment != null && existingEnrollment.Status == EnrollmentStatus.Active)
                throw new BadRequestException("You are already enrolled in this course.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                Enrollment createdEnrollment;

                if (existingEnrollment != null)
                {
                    // Reactivate a dropped/completed enrollment
                    existingEnrollment.Status = EnrollmentStatus.Active;
                    existingEnrollment.EnrolledAt = DateTime.UtcNow;
                    existingEnrollment.UpdatedAt = DateTime.UtcNow;
                    existingEnrollment.AmountPaid = 0;
                    existingEnrollment.OrderId = null;
                    await _unitOfWork.Enrollments.UpdateAsync(existingEnrollment, cancellationToken);
                    createdEnrollment = existingEnrollment;
                }
                else
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = studentId,
                        CourseId = request.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        Status = EnrollmentStatus.Active,
                        AmountPaid = 0,
                        OrderId = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    createdEnrollment = await _unitOfWork.Enrollments.AddAsync(enrollment, cancellationToken);
                }

                // Increment CurrentEnrollmentCount transactionally
                course.CurrentEnrollmentCount += 1;
                await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Successfully enrolled student (free course). EnrollmentId: {EnrollmentId}, StudentId: {StudentId}, CourseId: {CourseId}",
                    createdEnrollment.Id, studentId, request.CourseId);

                // Notify teacher about new enrollment
                try
                {
                    var student = await _unitOfWork.Users.GetUserByIdAsync(studentId, ct: cancellationToken);
                    await _notificationService.NotifyNewEnrollmentAsync(
                        course.TeacherId,
                        student?.FirstName ?? "A student",
                        course.Title,
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send enrollment notification");
                }

                return createdEnrollment.Id;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
