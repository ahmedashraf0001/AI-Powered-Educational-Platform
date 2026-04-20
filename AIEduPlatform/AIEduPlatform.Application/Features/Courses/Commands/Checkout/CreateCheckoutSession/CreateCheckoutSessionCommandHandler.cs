using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.DTOs.Payments;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Checkout.CreateCheckoutSession
{
    public class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CheckoutResponseDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IStripeService _stripeService;
        private readonly IUserTagService _userTagService;
        private readonly INotificationService _notificationService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CreateCheckoutSessionCommandHandler> _logger;

        public CreateCheckoutSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            IStripeService stripeService,
            IUserTagService userTagService,
            INotificationService notificationService,
            IConfiguration configuration,
            ILogger<CreateCheckoutSessionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _stripeService = stripeService;
            _userTagService = userTagService;
            _notificationService = notificationService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<CheckoutResponseDto> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId
                ?? throw new UnauthorizedException("You must be logged in to checkout.");

            var cart = await _unitOfWork.Carts.GetActiveCartByUserIdAsync(userId, cancellationToken)
                ?? throw new BadRequestException("Your cart is empty.");

            if (cart.Items == null || !cart.Items.Any())
                throw new BadRequestException("Your cart is empty.");

            // Validate all courses still exist and are published
            foreach (var item in cart.Items)
            {
                var course = item.Course ?? await _unitOfWork.Courses.GetByIdAsync(item.CourseId, cancellationToken);
                if (course == null || !course.IsPublished)
                    throw new BadRequestException($"Course '{item.Course?.Title ?? item.CourseId.ToString()}' is no longer available.");

                // Check not already enrolled
                var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(userId, item.CourseId, cancellationToken);
                if (isEnrolled)
                    throw new BadRequestException($"You are already enrolled in '{course.Title}'.");
            }

            var total = cart.Items.Sum(i => i.PriceAtTimeOfAdding);

            // Build the checkout items list up-front (used in both free and paid paths)
            var checkoutItems = cart.Items.Select(i => new CheckoutItemDto
            {
                CourseId = i.CourseId,
                CourseTitle = i.Course?.Title ?? string.Empty,
                Price = i.PriceAtTimeOfAdding
            }).ToList();

            // For PAID checkout: call Stripe BEFORE opening the DB transaction.
            // This ensures the Order is always persisted even if Stripe fails on a retry,
            // and avoids rolling back all DB writes due to a transient Stripe error.
            CreatePaymentIntentDto? paymentResult = null;
            if (total > 0)
            {
                paymentResult = await _stripeService.CreatePaymentIntentAsync(
                    (long)(total * 100), // Convert to cents
                    "usd",
                    new Dictionary<string, string>
                    {
                        { "UserId", userId.ToString() },
                        { "CartId", cart.Id.ToString() }
                    });
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                // Create Order — include Stripe IDs immediately so no second UpdateAsync is needed
                var order = new Order
                {
                    UserId = userId,
                    CartId = cart.Id,
                    TotalAmount = total,
                    Currency = "usd",
                    Status = total == 0 ? OrderStatus.Paid : OrderStatus.Pending,
                    PaidAt = total == 0 ? DateTime.UtcNow : null,
                    StripePaymentIntentId = paymentResult?.PaymentIntentId,
                    StripePaymentIntentClientSecret = paymentResult?.ClientSecret
                };

                order = await _unitOfWork.Orders.AddAsync(order, cancellationToken);

                // Create OrderItems
                foreach (var cartItem in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        CourseId = cartItem.CourseId,
                        Price = cartItem.PriceAtTimeOfAdding
                    };
                    await _unitOfWork.OrderItems.AddAsync(orderItem, cancellationToken);
                }

                if (total == 0)
                {
                    // Free checkout — enroll immediately
                    var enrolledCourseIds = new HashSet<Guid>();

                    foreach (var cartItem in cart.Items)
                    {
                        var existingEnrollment = await _unitOfWork.Enrollments.GetEnrollmentAsync(
                            userId, cartItem.CourseId, cancellationToken);

                        if (existingEnrollment != null)
                        {
                            // Reactivate — clear all unenrollment/refund fields
                            existingEnrollment.Status = EnrollmentStatus.Active;
                            existingEnrollment.EnrolledAt = DateTime.UtcNow;
                            existingEnrollment.OrderId = order.Id;
                            existingEnrollment.AmountPaid = 0;
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
                                StudentId = userId,
                                CourseId = cartItem.CourseId,
                                EnrolledAt = DateTime.UtcNow,
                                Status = EnrollmentStatus.Active,
                                OrderId = order.Id,
                                AmountPaid = 0
                            };
                            await _unitOfWork.Enrollments.AddAsync(enrollment, cancellationToken);
                        }

                        var course = cartItem.Course ?? await _unitOfWork.Courses.GetByIdAsync(cartItem.CourseId, cancellationToken);
                        if (course != null)
                        {
                            course.CurrentEnrollmentCount += 1;
                            await _unitOfWork.Courses.UpdateAsync(course, cancellationToken);
                        }

                        enrolledCourseIds.Add(cartItem.CourseId);
                    }

                    if (enrolledCourseIds.Count > 1)
                    {
                        await _userTagService.ApplyBatchCourseEnrollmentsAsync(
                            userId,
                            enrolledCourseIds,
                            cancellationToken);
                    }
                    else if (enrolledCourseIds.Count == 1)
                    {
                        await _userTagService.ApplyCourseEnrollmentAsync(
                            userId,
                            enrolledCourseIds.First(),
                            cancellationToken);
                    }
                }

                // Mark cart as checked out
                cart.Status = CartStatus.CheckedOut;
                await _unitOfWork.Carts.UpdateAsync(cart, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                if (total == 0)
                {
                    _logger.LogInformation("Free checkout completed. OrderId: {OrderId}, UserId: {UserId}", order.Id, userId);

                    // Notify teachers about new enrollments
                    try
                    {
                        var student = await _unitOfWork.Users.GetUserByIdAsync(userId, ct: cancellationToken);
                        foreach (var cartItem in cart.Items)
                        {
                            var course = cartItem.Course ?? await _unitOfWork.Courses.GetByIdAsync(cartItem.CourseId, cancellationToken);
                            if (course != null)
                            {
                                await _notificationService.NotifyNewEnrollmentAsync(
                                    course.TeacherId,
                                    student?.FirstName ?? "A student",
                                    course.Title,
                                    cancellationToken);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send free checkout enrollment notifications");
                    }

                    return new CheckoutResponseDto
                    {
                        OrderId = order.Id,
                        RequiresPayment = false,
                        TotalAmount = 0,
                        Currency = "usd",
                        Items = checkoutItems
                    };
                }

                // Paid checkout — Stripe PI was already created before the transaction
                _logger.LogInformation(
                    "Checkout session created. OrderId: {OrderId}, PaymentIntentId: {PaymentIntentId}, Total: {Total}",
                    order.Id, paymentResult!.PaymentIntentId, total);

                // Notify checkout initiated
                try
                {
                    await _notificationService.NotifyCheckoutSuccessAsync(
                        userId, total, order.Id, cart.Items.Count, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send checkout notification");
                }

                return new CheckoutResponseDto
                {
                    OrderId = order.Id,
                    ClientSecret = paymentResult.ClientSecret,
                    PaymentIntentId = paymentResult.PaymentIntentId,
                    PublishableKey = _configuration["Stripe:PublishableKey"],
                    RequiresPayment = true,
                    TotalAmount = total,
                    Currency = "usd",
                    Items = checkoutItems
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during checkout. Rolling back transaction.");
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);

                // Cancel the orphaned Stripe PaymentIntent so it doesn't hold an auth on the customer's card
                if (paymentResult?.PaymentIntentId != null)
                {
                    try
                    {
                        await _stripeService.CancelPaymentIntentAsync(paymentResult.PaymentIntentId);
                    }
                    catch (Exception stripeEx)
                    {
                        _logger.LogError(stripeEx, "Failed to cancel Stripe PaymentIntent {PaymentIntentId} after checkout rollback", paymentResult.PaymentIntentId);
                    }
                }

                throw;
            }
        }
    }
}
