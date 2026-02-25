using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Core.Domain.Entities;
using AIEduPlatform.Core.Interfaces.Repositories;
using AIEduPlatform.Core.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AIEduPlatform.Application.Features.StudySessions.Commands.Sessions.StartSession
{
    public class StartSessionCommandHandler : IRequestHandler<StartSessionCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<StartSessionCommandHandler> _logger;

        public StartSessionCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService,
            ILogger<StartSessionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<Guid> Handle(StartSessionCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
                throw new UnauthorizedException("You must be logged in to start a study session.");

            var isEnrolled = await _unitOfWork.Enrollments.IsStudentEnrolledAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (!isEnrolled)
                throw new ForbiddenException("You must be enrolled in this course to start a study session.");

            var activeSession = await _unitOfWork.StudySessions.GetActiveSessionAsync(
                userId.Value, request.CourseId, cancellationToken);

            if (activeSession is not null)
            {
                _logger.LogInformation(
                    "Reusing active session {SessionId} for student {StudentId}, course {CourseId}",
                    activeSession.Id, userId.Value, request.CourseId);

                await _unitOfWork.StudySessions.UpdateLastActivityAsync(activeSession.Id, cancellationToken);
                return activeSession.Id;
            }

            var now = DateTime.UtcNow;
            var session = new StudySession
            {
                StudentId = userId.Value,
                CourseId = request.CourseId,
                StartedAt = now,
                LastActivity = now,
                CreatedAt = now,
                UpdatedAt = now
            };

            var created = await _unitOfWork.StudySessions.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Created study session {SessionId} for student {StudentId}, course {CourseId}",
                created.Id, userId.Value, request.CourseId);

            return created.Id;
        }
    }
}
