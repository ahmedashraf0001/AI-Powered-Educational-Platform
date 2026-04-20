using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Domain.Enums;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using AIEduPlatform.Infrastructure.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Courses.Commands.Courses.DeleteCourse
{
    public class DeleteCourseCommandHandler : IRequestHandler<DeleteCourseCommand, DeleteCourseResult>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRAGService _ragService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteCourseCommandHandler> _logger;
        private readonly IFileService _fileService;
        public DeleteCourseCommandHandler(
            IUnitOfWork unitOfWork,
            IRAGService ragService,
            ICurrentUserService currentUserService,
            ILogger<DeleteCourseCommandHandler> logger,
            IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _ragService = ragService;
            _currentUserService = currentUserService;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<DeleteCourseResult> Handle(DeleteCourseCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to delete a course.");
            }

            _logger.LogInformation(
                "Deleting course. CourseId: {CourseId}, UserId: {UserId}",
                request.CourseId,
                userId.Value);

            try
            {
                var course = await _unitOfWork.Courses.GetCourseByIdAsync(request.CourseId, null, cancellationToken);

                if (course == null)
                {
                    _logger.LogWarning("Course not found. CourseId: {CourseId}", request.CourseId);
                    throw new NotFoundException(nameof(Course), request.CourseId);
                }

                if (course.TeacherId != userId.Value)
                {
                    _logger.LogWarning(
                        "User {UserId} is not authorized to delete course {CourseId}",
                        userId.Value,
                        request.CourseId);
                    throw new ForbiddenException("You are not authorized to delete this course.");
                }

                _logger.LogInformation(
                    "Found course to delete. CourseId: {CourseId}, Title: {Title}",
                    course.Id,
                    course.Title);

                var hasSalesHistory = await _unitOfWork.OrderItems.AnyAsync(
                    oi => oi.CourseId == request.CourseId,
                    cancellationToken);

                if (hasSalesHistory)
                {
                    var revokeAccess = ShouldRevokeAccess(request.Reason);
                    var revokedEnrollments = 0;

                    if (course.IsPublished)
                    {
                        course.IsPublished = false;
                        _unitOfWork.Courses.Update(course);
                    }

                    if (revokeAccess)
                    {
                        revokedEnrollments = await RevokeStudentAccessAsync(request.CourseId, cancellationToken);
                    }

                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var message = BuildSoldCourseMessage(course.Title, request.Reason, revokeAccess, revokedEnrollments);

                    _logger.LogInformation(
                        "Sold course {CourseId} was unpublished instead of deleted. Reason={Reason}, AccessRevoked={AccessRevoked}, RevokedEnrollments={RevokedEnrollments}",
                        request.CourseId,
                        request.Reason,
                        revokeAccess,
                        revokedEnrollments);

                    return new DeleteCourseResult
                    {
                        PermanentlyDeleted = false,
                        Unpublished = true,
                        AccessRevoked = revokeAccess,
                        Message = message
                    };
                }

                // RAG service deletes both the course and its chunks
                var fileUrls = await _unitOfWork.Materials.GetMaterialFileUrlsByCourseIdAsync(request.CourseId, cancellationToken);

                await Task.WhenAll(
                    fileUrls
                        .Where(f => !string.IsNullOrEmpty(f))
                        .Select(f =>  _fileService.DeleteFileAsync(f, cancellationToken))
                );

                var ragDeleteResult = await _ragService.DeleteCourseAsync(request.CourseId, cancellationToken);

                if (!ragDeleteResult.Success)
                {
                    _logger.LogError(
                        "Failed to delete course {CourseId}: {Error}",
                        request.CourseId,
                        ragDeleteResult.Error);
                    throw new InvalidOperationException($"Failed to delete course: {ragDeleteResult.Error}");
                }

                _logger.LogInformation(
                    "Successfully deleted course. CourseId: {CourseId}, Title: {Title}",
                    request.CourseId,
                    course.Title);

                return new DeleteCourseResult
                {
                    PermanentlyDeleted = true,
                    Unpublished = false,
                    AccessRevoked = false,
                    Message = $"Course \"{course.Title}\" was permanently deleted."
                };
            }
            catch (Exception ex) when (ex is not NotFoundException and not ForbiddenException and not UnauthorizedException)
            {
                _logger.LogError(ex, "Error deleting course. CourseId: {CourseId}", request.CourseId);
                throw;
            }
        }

        private static bool ShouldRevokeAccess(CourseRemovalReason reason)
        {
            return reason is CourseRemovalReason.PolicyViolation or CourseRemovalReason.LegalRequest;
        }

        private async Task<int> RevokeStudentAccessAsync(Guid courseId, CancellationToken cancellationToken)
        {
            var enrollments = await _unitOfWork.Enrollments.GetEnrollmentsByCourseAsync(courseId, includeStudent: false, cancellationToken);
            var now = DateTime.UtcNow;

            var toRevoke = enrollments
                .Where(e => e.Status is EnrollmentStatus.Active or EnrollmentStatus.Completed)
                .ToList();

            foreach (var enrollment in toRevoke)
            {
                enrollment.Status = EnrollmentStatus.Dropped;
                enrollment.UnenrolledAt ??= now;
            }

            if (toRevoke.Count > 0)
            {
                _unitOfWork.Enrollments.UpdateRange(toRevoke);
            }

            return toRevoke.Count;
        }

        private static string BuildSoldCourseMessage(
            string courseTitle,
            CourseRemovalReason reason,
            bool accessRevoked,
            int revokedEnrollments)
        {
            var safeTitle = string.IsNullOrWhiteSpace(courseTitle) ? "this course" : courseTitle;

            if (!accessRevoked)
            {
                return $"Course \"{safeTitle}\" has purchase history, so it was unpublished instead of deleted. Existing student access was preserved and transaction records were kept.";
            }

            var reasonText = reason == CourseRemovalReason.PolicyViolation ? "policy violation" : "legal/compliance request";
            return $"Course \"{safeTitle}\" has purchase history, so it was unpublished instead of deleted. Access was revoked for {revokedEnrollments} enrollment(s) due to {reasonText}. Transaction records were kept.";
        }
    }
}
