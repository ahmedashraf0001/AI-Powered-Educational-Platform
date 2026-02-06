using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.Exams.Commands.Exams.CreateExam
{
    public class CreateExamCommandHandler : IRequestHandler<CreateExamCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateExamCommandHandler> _logger;

        public CreateExamCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<CreateExamCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Guid> Handle(CreateExamCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (!userId.HasValue)
            {
                throw new UnauthorizedException("You must be logged in to create an exam.");
            }

            var course = await _unitOfWork.Courses.GetByIdAsync(request.CourseId, cancellationToken);

            if (course == null)
            {
                _logger.LogWarning("Course {CourseId} not found for exam creation by user {UserId}.", request.CourseId, userId.Value);
                throw new NotFoundException(nameof(Course), request.CourseId);
            }

            if (course.TeacherId != userId.Value)
            {
                _logger.LogWarning("User {UserId} attempted to create exam for course {CourseId} without permission.", userId.Value, request.CourseId);
                throw new ForbiddenException("You are not authorized to create an exam for this course.");
            }

            _logger.LogInformation(
                "Creating new exam. Title: {Title}, CourseId: {CourseId}, UserId: {UserId}",
                request.Title,
                request.CourseId,
                userId.Value);
            try
            {
                var exam = new Exam
                {
                    CourseId = request.CourseId,
                    Title = request.Title,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    DurationMinutes = request.DurationMinutes
                };
                await _unitOfWork.Exams.AddAsync(exam, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Exam created successfully. ExamId: {ExamId}, Title: {Title}, CourseId: {CourseId}, UserId: {UserId}",
                    exam.Id,
                    request.Title,
                    request.CourseId,
                    userId.Value);
                return exam.Id;

            }
            catch (Exception ex) when (!(ex is UnauthorizedException or UnauthorizedAccessException))
            {
                _logger.LogError(
                    ex,
                    "Error creating exam. Title: {Title}, CourseId: {CourseId}, UserId: {UserId}",
                    request.Title,
                    request.CourseId,
                    userId.Value);
                throw;
            }
        }
    }
}
