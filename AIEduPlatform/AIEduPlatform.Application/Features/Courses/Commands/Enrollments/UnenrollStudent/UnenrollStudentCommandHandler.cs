using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Enrollments;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Enrollments.UnenrollStudent
{
    public class UnenrollStudentCommandHandler : IRequestHandler<UnenrollStudentCommand, UnenrollmentResultDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IStripeService _stripeService;
        private readonly ILogger<UnenrollStudentCommandHandler> _logger;

        public UnenrollStudentCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            IStripeService stripeService,
            ILogger<UnenrollStudentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _stripeService = stripeService;
            _logger = logger;
        }

        public async Task<UnenrollmentResultDto> Handle(UnenrollStudentCommand request, CancellationToken cancellationToken)
        {
            var studentId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to unenroll from a course.");

            _logger.LogInformation(
                "Unenrolling student from course. StudentId: {StudentId}, CourseId: {CourseId}",
                studentId, request.CourseId);

            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                studentId, request.CourseId, cancellationToken)
                ?? throw new NotFoundException("You are not enrolled in this course.");

            if (enrollment.Status != EnrollmentStatus.Active)
                throw new BadRequestException("You are not currently enrolled in this course.");

            // Rule 4: Already refunded/unenrolled
            if (enrollment.UnenrolledAt != null)
                throw new BadRequestException("You have already unenrolled from this course.");

            // Rule 3: Free courses — allow unenrollment freely
            if (enrollment.AmountPaid == 0)
            {
                enrollment.Status = EnrollmentStatus.Dropped;
                enrollment.UnenrolledAt = DateTime.UtcNow;
                enrollment.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Enrollments.UpdateAsync(enrollment, cancellationToken);

                // Decrement enrollment count
                var freeCourse = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);
                if (freeCourse != null)
                {
                    freeCourse.CurrentEnrollmentCount = Math.Max(0, freeCourse.CurrentEnrollmentCount - 1);
                    await _unitOfWork.Courses.UpdateAsync(freeCourse, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Free course unenrollment completed. StudentId: {StudentId}, CourseId: {CourseId}", studentId, request.CourseId);

                // Notifications
                try
                {
                    await _notificationService.NotifyUnenrollmentAsync(studentId, freeCourse?.Title ?? "", cancellationToken);
                    if (freeCourse != null)
                    {
                        var student = await _unitOfWork.Users.GetUserByIdAsync(studentId, ct: cancellationToken);
                        await _notificationService.NotifyStudentUnenrolledAsync(
                            freeCourse.TeacherId, student?.FirstName ?? "A student", freeCourse.Title, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send unenrollment notification");
                }

                return new UnenrollmentResultDto
                {
                    Success = true,
                    Message = "You have been unenrolled from the course."
                };
            }

            // Rule 1: Time window — 10 days
            var daysSinceEnrollment = (DateTime.UtcNow - enrollment.EnrolledAt).TotalDays;
            if (daysSinceEnrollment > 10)
            {
                return new UnenrollmentResultDto
                {
                    Success = false,
                    Message = "Unenrollment is no longer available. The 10-day window has passed.",
                    EnrolledAt = enrollment.EnrolledAt,
                    DeadlineWas = enrollment.EnrolledAt.AddDays(10)
                };
            }

            // Rule 2: Progress-based refund
            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId);

            // Calculate progress
            var completedMaterials = await _unitOfWork.MaterialProgress.CountAsync(
                mp => mp.StudentId == studentId && mp.Material != null &&
                      mp.Material.Lecture != null && mp.Material.Lecture.CourseId == request.CourseId &&
                      mp.IsCompleted, cancellationToken);

            var lectures = await _unitOfWork.Lectures.GetLecturesByCourseIdAsync(request.CourseId, cancellationToken);
            var totalLectures = lectures?.Count ?? 0;
            var completeLectures = 0;

            if (totalLectures > 0 && lectures != null)
            {
                foreach (var lecture in lectures)
                {
                    var materials = await _unitOfWork.Materials.FindAsync(m => m.LectureId == lecture.Id, cancellationToken);
                    var materialList = materials.ToList();
                    if (!materialList.Any()) continue;

                    var completedInLecture = await _unitOfWork.MaterialProgress.CountAsync(
                        mp => mp.StudentId == studentId && materialList.Select(m => m.Id).Contains(mp.MaterialId) && mp.IsCompleted,
                        cancellationToken);

                    if (completedInLecture >= materialList.Count)
                        completeLectures++;
                }
            }

            var progressPercentage = totalLectures > 0
                ? (double)completeLectures / totalLectures * 100
                : 0;

            decimal refundAmount;
            if (progressPercentage <= 50)
            {
                // Full refund
                refundAmount = enrollment.AmountPaid;
            }
            else
            {
                // 50% refund
                refundAmount = enrollment.AmountPaid * 0.5m;
            }

            // Round to 2 decimal places
            refundAmount = Math.Round(refundAmount, 2);

            string? stripeRefundId = null;

            // Process Stripe refund if applicable
            if (refundAmount > 0 && enrollment.OrderId.HasValue)
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(enrollment.OrderId.Value, cancellationToken);
                if (order != null && !string.IsNullOrEmpty(order.StripePaymentIntentId))
                {
                    try
                    {
                        stripeRefundId = await _stripeService.CreateRefundAsync(
                            order.StripePaymentIntentId,
                            (long)(refundAmount * 100),
                            "requested_by_customer");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Stripe refund failed for enrollment {EnrollmentId}", enrollment.Id);
                        throw new BadRequestException("Failed to process refund. Please try again or contact support.");
                    }
                }
            }

            // Update enrollment
            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.UnenrolledAt = DateTime.UtcNow;
            enrollment.RefundedAt = DateTime.UtcNow;
            enrollment.RefundAmount = refundAmount;
            enrollment.StripeRefundId = stripeRefundId;
            enrollment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Enrollments.UpdateAsync(enrollment, cancellationToken);

            // Decrement enrollment count
            course.CurrentEnrollmentCount = Math.Max(0, course.CurrentEnrollmentCount - 1);
            await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Unenrollment with refund completed. EnrollmentId: {EnrollmentId}, RefundAmount: {RefundAmount}, StripeRefundId: {StripeRefundId}",
                enrollment.Id, refundAmount, stripeRefundId ?? "N/A");

            // Notifications
            try
            {
                if (refundAmount > 0)
                {
                    await _notificationService.NotifyUnenrollmentWithRefundAsync(
                        studentId, course.Title, refundAmount, cancellationToken);
                }
                else
                {
                    await _notificationService.NotifyUnenrollmentAsync(studentId, course.Title, cancellationToken);
                }

                var student = await _unitOfWork.Users.GetUserByIdAsync(studentId, ct: cancellationToken);
                await _notificationService.NotifyStudentUnenrolledAsync(
                    course.TeacherId, student?.FirstName ?? "A student", course.Title, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send unenrollment notification");
            }

            return new UnenrollmentResultDto
            {
                Success = true,
                Message = "You have been unenrolled from the course.",
                RefundAmount = refundAmount,
                RefundCurrency = "usd",
                RefundEta = refundAmount > 0 ? "5-10 business days" : null,
                StripeRefundId = stripeRefundId
            };
        }
    }
}
