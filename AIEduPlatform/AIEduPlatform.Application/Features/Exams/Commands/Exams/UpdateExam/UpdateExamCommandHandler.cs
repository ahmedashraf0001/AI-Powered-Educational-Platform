using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.UpdateExam
{
    public class UpdateExamCommandHandler : IRequestHandler<UpdateExamCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateExamCommandHandler> _logger;

        public UpdateExamCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<UpdateExamCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Unit> Handle(UpdateExamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedException("You must be logged in to update an exam.");

            // Single query to get exam with course - reduces 2 DB hits to 1
            var exam = await _unitOfWork.Exams.GetExamWithCourseAsync(request.ExamId, cancellationToken);
            if (exam == null)
            {
                _logger.LogWarning("Exam {ExamId} not found for update by user {UserId}.", request.ExamId, userId);
                throw new NotFoundException($"Exam with ID {request.ExamId} not found.");
            }
            var course = exam.Course;
            var teacherId = course?.TeacherId;
            if (teacherId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to update exam {ExamId} without permission.", userId, request.ExamId);
                throw new UnauthorizedAccessException("You do not have permission to update this exam.");
            }
            try
            {
                // Re-fetch for tracking since we used AsNoTracking
                var examToUpdate = await _unitOfWork.Exams.GetByIdAsync(request.ExamId, cancellationToken);
                examToUpdate!.Title = request.Title;
                examToUpdate.StartTime = request.StartTime;
                examToUpdate.EndTime = request.EndTime;
                examToUpdate.DurationMinutes = request.DurationMinutes;
                await _unitOfWork.Exams.UpdateAsync(examToUpdate, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Exam {ExamId} updated successfully by user {UserId}.", request.ExamId, userId);
                return Unit.Value;
            }
            catch (Exception ex) when (ex is not (UnauthorizedException or ForbiddenException or NotFoundException))
            {
                _logger.LogError(ex, "An error occurred while updating exam {ExamId} by user {UserId}.", request.ExamId, userId);
                throw new Exception("An error occurred while updating the exam. Please try again later.");
            }
        }
    }
}
