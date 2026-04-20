using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Payments.ConfirmPayment
{
    /// <summary>
    /// Handles payment confirmation from Stripe webhook.
    /// Looks up the Order by StripePaymentIntentId, marks it Paid,
    /// creates enrollments, increments course counts, and notifies.
    /// </summary>
    public class ConfirmPaymentCommandHandler : IRequestHandler<ConfirmPaymentCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserTagService _userTagService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ConfirmPaymentCommandHandler> _logger;

        public ConfirmPaymentCommandHandler(
            IUnitOfWork unitOfWork,
            IUserTagService userTagService,
            INotificationService notificationService,
            ILogger<ConfirmPaymentCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _userTagService = userTagService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(ConfirmPaymentCommand request, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.Orders.GetByStripePaymentIntentIdAsync(request.PaymentIntentId, cancellationToken);
            if (order == null)
            {
                _logger.LogWarning("No Order found for PaymentIntentId: {PaymentIntentId}", request.PaymentIntentId);
                return Unit.Value;
            }

            await HandleOrderBasedPayment(order, cancellationToken);
            return Unit.Value;
        }

        private async Task HandleOrderBasedPayment(Order order, CancellationToken cancellationToken)
        {
            // Idempotency guard
            if (order.Status == OrderStatus.Paid)
            {
                _logger.LogWarning("Order {OrderId} already paid. Skipping.", order.Id);
                return;
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Update order status
                order.Status = OrderStatus.Paid;
                order.PaidAt = DateTime.UtcNow;
                await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);

                var courseNames = new List<string>();
                var enrolledCourseIds = new HashSet<Guid>();

                // Create enrollments for each order item
                foreach (var orderItem in order.Items ?? [])
                {
                    // Idempotency: check if already enrolled
                    var existingEnrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                        order.UserId, orderItem.CourseId, cancellationToken);

                    if (existingEnrollment != null && existingEnrollment.Status == EnrollmentStatus.Active)
                    {
                        courseNames.Add(orderItem.Course?.Title ?? "Unknown");
                        continue;
                    }

                    if (existingEnrollment != null)
                    {
                        // Reactivate — clear all unenrollment/refund fields
                        existingEnrollment.Status = EnrollmentStatus.Active;
                        existingEnrollment.EnrolledAt = DateTime.UtcNow;
                        existingEnrollment.OrderId = order.Id;
                        existingEnrollment.AmountPaid = orderItem.Price;
                        existingEnrollment.UnenrolledAt = null;
                        existingEnrollment.RefundedAt = null;
                        existingEnrollment.RefundAmount = null;
                        existingEnrollment.StripeRefundId = null;
                        existingEnrollment.UpdatedAt = DateTime.UtcNow;
                        await _unitOfWork.Enrollments.UpdateAsync(existingEnrollment, cancellationToken);
                    }
                    else
                    {
                        var enrollment = new Enrollment
                        {
                            StudentId = order.UserId,
                            CourseId = orderItem.CourseId,
                            EnrolledAt = DateTime.UtcNow,
                            Status = EnrollmentStatus.Active,
                            OrderId = order.Id,
                            AmountPaid = orderItem.Price,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        await _unitOfWork.Enrollments.AddAsync(enrollment, cancellationToken);
                    }

                    enrolledCourseIds.Add(orderItem.CourseId);

                    // Increment enrollment count
                    var course = orderItem.Course ?? await _unitOfWork.Courses.GetByIdAsync(orderItem.CourseId, cancellationToken);
                    if (course != null)
                    {
                        course.CurrentEnrollmentCount += 1;
                        await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                        courseNames.Add(course.Title);

                        // Notify teacher
                        try
                        {
                            var student = await _unitOfWork.Users.GetUserByIdAsync(order.UserId, ct: cancellationToken);
                            await _notificationService.NotifyNewEnrollmentAsync(
                                course.TeacherId,
                                student?.FirstName ?? "A student",
                                course.Title,
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to notify teacher about enrollment for course {CourseId}", course.Id);
                        }
                    }
                }

                if (enrolledCourseIds.Count > 1)
                {
                    await _userTagService.ApplyBatchCourseEnrollmentsAsync(
                        order.UserId,
                        enrolledCourseIds,
                        cancellationToken);
                }
                else if (enrolledCourseIds.Count == 1)
                {
                    await _userTagService.ApplyCourseEnrollmentAsync(
                        order.UserId,
                        enrolledCourseIds.First(),
                        cancellationToken);
                }

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                _logger.LogInformation(
                    "Order payment confirmed. OrderId: {OrderId}, UserId: {UserId}, Courses: {CourseCount}",
                    order.Id, order.UserId, courseNames.Count);

                // Notify student about payment success
                try
                {
                    await _notificationService.NotifyPaymentSuccessAsync(
                        order.UserId, order.TotalAmount, order.Id, courseNames, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send payment success notification");
                }
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}
