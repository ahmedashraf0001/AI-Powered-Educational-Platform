using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.DeleteExam
{
    public class DeleteExamCommandHandler : IRequestHandler<DeleteExamCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<DeleteExamCommandHandler> _logger;

        public DeleteExamCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            INotificationService notificationService,
            ILogger<DeleteExamCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteExamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized attempt to delete exam {ExamId}.", request.ExamId);
                throw new UnauthorizedException("You must be logged in to delete an exam.");
            }

            // Single query to get exam with course - reduces 2 DB hits to 1
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);
            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for deletion by user {UserId}.", request.ExamId, userId);
                throw new NotFoundException($"Exam with ID {request.ExamId} not found.");
            }
            var teacherId = exam.Course?.TeacherId;
            if (teacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to delete exam {ExamId} without authorization.", userId, request.ExamId);
                throw new UnauthorizedAccessException("You are not authorized to create an exam for this course.");
            }
            try
            {
                // Re-fetch for tracking since we used AsNoTracking
                var examToDelete = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);
                await _unitOfWork.Exams.DeleteAsync(examToDelete!, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Notify students that exam was cancelled
                await _notificationService.NotifyExamDeletedAsync(
                    exam.CourseId,
                    exam.Course?.Title ?? "Course",
                    exam.Title,
                    cancellationToken);

                return Unit.Value;

            }
            catch (Exception ex) when (!(ex is UnauthorizedException or UnauthorizedAccessException or NotFoundException))
            {
                _logger.LogError(ex, "An error occurred while deleting the exam with ID {ExamId}.", request.ExamId);
                throw new Exception("An error occurred while deleting the exam. Please try again later.");
            }
        }
    }
}
